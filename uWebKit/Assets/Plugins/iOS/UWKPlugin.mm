/******************************************
  * uWebKit 
  * (c) 2013 THUNDERBEAST GAMES, LLC
  * sales@uwebkit.com
*******************************************/

#import "UWKPlugin.h"
#import "UWKWebEngine.h"

extern "C" {
// On Windows/OSX uWebKit uses shared memory to transfer larger data stores between native and unmanaged
// On iOS, these buffers need to be consistent and are handled in process
#define MAX_BUFFER 1024
bool sBufferFlags[MAX_BUFFER];
const unsigned char *sBuffers[MAX_BUFFER];
unsigned int sBufferSizes[MAX_BUFFER];


// allocate a data buffer of given size, copy buffer into it, and return an id
// TODO, better error checking
int AllocateAndCopy(const unsigned char *buffer, int size) {
	int i;

	for(i = 0; i < MAX_BUFFER; i++) {
		if(!sBufferFlags[i]) {
			sBuffers[i] = (unsigned char * )malloc(size);
			memcpy((void *)sBuffers[i], (void *)buffer, size);
			sBufferSizes[i] = size;
			sBufferFlags[i] = true;
			return i;
		}
	}

	return -1;
}

// retrieve the data buffer to dataOut, verifying size, and free buffer once retrieved
bool CopyAndFree(int idx, unsigned char *dataOut, int size) {
	if(idx >= MAX_BUFFER || idx < 0)
		return false;

	if(!sBufferFlags[idx])
		return false;

	if(size != sBufferSizes[idx])
		return false;             // this is a serious error

	memcpy(dataOut, sBuffers[idx], size);

	free((void *)sBuffers[idx]);
	sBufferFlags[idx] = false;

	return true;
}

// NSString -> Unicode string handling
void SetSParam(Command *cmd, int index, NSString *value) {
	NSData *bytes = [value dataUsingEncoding:NSUTF16LittleEndianStringEncoding];

	memset(cmd->sParams[index], 0, 256 * sizeof(unsigned short));
	memcpy(cmd->sParams[index], [bytes bytes], [bytes length]);
	// [bytes release];
}

// CString -> Unity string handling
void SetSParamCString(Command *cmd, int index, const char *value) {
	NSString *svalue = [NSString stringWithUTF8String:value];

	SetSParam(cmd, index, svalue);
	// [svalue release];
}

// Retrieve the string command parameter as a ObjC string
NSString * GetSParam(Command *cmd, int index) {
	int slength = 0;

	while(cmd->sParams[index][slength] != 0)
		slength++;

	NSString *param = [[NSString alloc] initWithBytes:cmd->sParams[index]
											   length:slength * 2
											 encoding:NSUTF16LittleEndianStringEncoding];

	return param;
}

void  UWK_SetLogCB(LOGCB lcb) {
}

void  UWK_SetProcessCB(CBPROCESSCOMMAND pcb) {
}

// uWebKit uses a command structure to pass data/commands between
// managed and unmanaged code, on Windows/OSX these commands are
// passed between the Unity and uWebKit processes, on iOS this is
// handled in process

static unsigned int sCommandIdCount = 1;
static Command sCommands[256];
static int sActiveCommandCount = 0;

// initialize a command with basic info and given command code
void InitCommand(Command *cmd, const char *fourcc) {
	memset(cmd, 0, sizeof(Command));

	strcpy(cmd->fourcc, fourcc);
	cmd->id = sCommandIdCount++;
	cmd->src = PROCESS;

	if(sCommandIdCount == 0)
		sCommandIdCount = 1;
}

// post a command to the command buffer (this can be a new command or a return value)
// NATIVE -> MANAGED
unsigned int PostCommand(Command *cmd) {
	if(sActiveCommandCount == 256) {
		printf_console("-> Command Overrun\n");
		return 0;
	}

	Command *scmd = &sCommands[sActiveCommandCount++];

	memcpy(scmd, cmd, sizeof(Command));

	if(cmd->retCode == 0) {
		scmd->id = sCommandIdCount++;

		if(sCommandIdCount == 0)
			sCommandIdCount = 1;

		return scmd->id;
	}

	return scmd->id;
}

// MANAGED -> NATIVE
unsigned int UWK_PostCommand(Command *command) {
	return PostCommand(command);
}

// managed -> native wrapper
int  UWK_AllocateAndCopy(unsigned char *addr, int size) {
	return AllocateAndCopy(addr, size);
}

// managed -> native wrapper
bool  UWK_CopyAndFree(int idx, unsigned char *out, int size) {
	return CopyAndFree(idx, out, size);
}

// initialize the native side and post a "process up" command back to managed
bool  UWK_Init() {
	memset(sBufferFlags, 0, sizeof(bool) * MAX_BUFFER);

	Command cmd;
	InitCommand(&cmd, "PRUP");
	PostCommand(&cmd);

	return true;
}

// consistent with Window/OSX (where we launch the web process)
void  UWK_InitProcess() {
	Command cmd;

	InitCommand(&cmd, "INIT");
	cmd.src = PLUGIN;
	cmd.iParams[0] = 0;
	cmd.numIParams = 1;
	PostCommand(&cmd);
}

// command processing happens in batches on iOS
static Command sBatchCommands[256];
static int sBatchCommandCount = 0;
static int sCurrentBatchCommand = 0;

// called once per frame to update views and process commands
int UWK_Update(Command *ocmd) {
	[[UWKWebEngine sInstance] tick];

	if(!sBatchCommandCount && sActiveCommandCount > 0) {
		sBatchCommandCount = sActiveCommandCount;
		sCurrentBatchCommand = 0;
		memcpy(sBatchCommands, sCommands, sizeof(Command) * sActiveCommandCount);
		sActiveCommandCount = 0;
	} else if(sCurrentBatchCommand >= sBatchCommandCount) {
		sBatchCommandCount = 0;
	}

	if(!sBatchCommandCount)
		return 0;

	while(sCurrentBatchCommand < sBatchCommandCount) {
		Command *cmd = &sBatchCommands[sCurrentBatchCommand++];

		if(cmd->src == PLUGIN && !cmd->retCode) {
			[UWKWebEngine.sInstance processCommand:cmd];

			// printf_console("Plugin -> Process: %s\n", cmd->fourcc);
		}
		// return
		else if(cmd->src == PLUGIN && cmd->retCode) {
			// printf_console("Plugin -> Process Return: %s\n", cmd->fourcc);
			memcpy(ocmd, cmd, sizeof(Command));
			return 1;
		}
		// post new
		else if(cmd->src == PROCESS && !cmd->retCode) {
			// printf_console("Process -> Plugin: %s\n", cmd->fourcc);
			memcpy(ocmd, cmd, sizeof(Command));
			return 1;
		} else if(cmd->src == PROCESS && cmd->retCode) {
			if(!strncmp(cmd->fourcc, "BOOT", 4)) {
				// We've booted
				UWKWebEngine.sInstance.initialized = true;
			}

			// Process Return
			// printf_console("Process -> Plugin Return: %s\n", cmd->fourcc);
		}
	}

	sBatchCommandCount = 0;
	return 0;
}

void  UWK_Shutdown() {
}

void  UWK_ClearCommands() {
}

// we use native UIWebView on iOS for consistent UI/touch handling
// there is no need to blit to a texture unless using GLES (see capture command)
void   UWK_UpdateTexture(bool isBackBuffer, int mip, int child, void *buffer) {
}
}
