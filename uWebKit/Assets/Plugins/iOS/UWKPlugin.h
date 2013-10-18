/******************************************
  * uWebKit 
  * (c) 2013 THUNDERBEAST GAMES, LLC
  * sales@uwebkit.com
*******************************************/

extern "C" {
// uWebKit uses a command structure to pass data/commands between
// managed and unmanaged code, on Windows/OSX these commands are
// passed between the Unity and uWebKit processes, on iOS this is
// handled in process

// A command's source will either be from the plugin (Unity) or from the Process (native)
enum Source { PLUGIN, PROCESS, FORCE32 = 0xFFFFFFFF };


// callback proto for logging
typedef void (*LOGCB)(const char *);

typedef struct Command_s   Command;

// callbacks for error, success, and processing
typedef void (*CBERROR)(Command *cmd);
typedef void (*CBSUCCESS)(Command *cmd);
typedef void (*CBPROCESSCOMMAND)(Command *m);

// message class blittable from C#
struct Command_s {
	// id of 0 = free (idCounter stored in first 4 bytes of command buffer
	unsigned int id;

	// 4 bytes + null term string id
	char fourcc[5];

	// where the command originates, either Unity (plugin) or Native (process)
	enum Source src;

	// params in, also out for successcb/errorcb (destructive)

	// int parameters
	int iParams[16];
	int numIParams;

	// string parameters
	unsigned short sParams[16][256];
	int numSParams;

	// < 0 error, 0 == unprocessed, > 0 = success
	int retCode;

	// success and error callbacks
	CBSUCCESS cbSuccess;
	CBERROR cbError;

	// if valid, instance who owns this message
	void *pthis;
};


// NSString -> Unicode string handling
extern void SetSParam(Command *cmd, int index, NSString *value);

// CString -> Unity string handling
extern void SetSParamCString(Command *cmd, int index, const char *value);

// Retrieve the string command parameter as a ObjC string
extern NSString * GetSParam(Command *cmd, int index);

// initialize a command with basic info and given command code
extern void InitCommand(Command *cmd, const char *fourcc);

// post a command to the command buffer (this can be a new command or a return value)
extern unsigned int PostCommand(Command *cmd);

// On Windows/OSX uWebKit uses shared memory to transfer larger data stores between native and unmanaged
// On iOS, these buffers need to be consistent and are handled in process

// allocate a data buffer of given size, copy buffer into it, and return an id
int AllocateAndCopy(const unsigned char *buffer, int size);

// retrieve the data buffer to dataOut, verifying size, and free buffer once retrieved
bool CopyAndFree(int idx, unsigned char *dataOut, int size);
}