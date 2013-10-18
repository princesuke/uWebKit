/******************************************
  * uWebKit 
  * (c) 2013 THUNDERBEAST GAMES, LLC
  * sales@uwebkit.com
*******************************************/

#import "UWKWebEngine.h"
#import "UWKWebView.h"
#import "UWKPlugin.h"
#import "../Classes/UnityAppController.h"  

// The Unity Window
extern UIWindow *_window;

@implementation UWKWebEngine

// properties
@synthesize appController;
@synthesize viewLookup;
@synthesize initialized;
@synthesize keyboardVisible;

// The Unity Window
extern UIWindow *_window;

// retrieve the UWKWindow with the given name
- (UWKWebView *)getWebView:(NSString *)name {
	return [viewLookup objectForKey:name];
}

// remove (and release) the named UWKWindow
- (void)removeWebView:(NSString *)name {
	// this releases view as we don't retain
	[viewLookup removeObjectForKey:name];
}

// Add a new web view with the given name
- (UWKWebView *)addWebView:(NSString *)name width:(int)width height:(int)height smartRects:(bool)smartRects {
	UWKWebView *view = [self getWebView:name];

	if(view != nil)
		return nil;

	view = [UWKWebView alloc];
	[view init:name width:width height:height smartRects:smartRects];

	[viewLookup setObject:view forKey:name];

	return view;
}

// update the views per tick
- (void)tick {
	if(!initialized)
		return;

	NSEnumerator *enumerator = [viewLookup keyEnumerator];

	NSString *key;

	while((key = [enumerator nextObject])) {
		UWKWebView *view = [viewLookup objectForKey:key];

		[view update];
	}
}

// process inbound commands from Unity
- (void)processCommand:(Command *)cmd {
	if(!strncmp(cmd->fourcc, "INIT", 4)) {
		// We have received the init command which is
		// expecting a BOOT from the native side

		Command post;
		InitCommand(&post, "BOOT");

		// No Error
		post.iParams[0] = 0;

		// we are deployed and not running in editor
		post.iParams[1] = 1;
		post.iParams[2] = 1;
		post.iParams[3] = 0;

		post.numIParams = 4;

		// TODO: iOS version string info
		SetSParamCString(&post, 0, "IOS");
		SetSParamCString(&post, 1, "IOS");
		SetSParamCString(&post, 2, "IOS");

		post.numSParams = 3;

		// post it
		PostCommand(&post);
	} else if(!strncmp(cmd->fourcc, "CRVW", 4)) {
		// Create a new web view

		bool smartRects = cmd->iParams[2] ? true : false;

		NSString *name = GetSParam(cmd, 0);

		UWKWebView *view =
			[self addWebView:name width:cmd->iParams[0] height:cmd->iParams[1] smartRects:smartRects];

		if(view == nil)
			cmd->retCode = -1;
		else
			cmd->retCode = 1;

		PostCommand(cmd);

		[name release];
	} else if(!strncmp(cmd->fourcc, "RMVW", 4)) {
		// Remove a web view

		NSString *name = GetSParam(cmd, 0);

		UWKWebView *view = [self getWebView:name];

		if(view != nil) {
			[self removeWebView:name];
		}
	} else if(!strncmp(cmd->fourcc, "SHOW", 4)) {
		// Show/Hide a web view

		NSString *name = GetSParam(cmd, 0);

		UWKWebView *view = [self getWebView:name];

		if(view != nil) {
			[view show:cmd->iParams[0] == 1];
		}
	} else if(!strncmp(cmd->fourcc, "TRAN", 4)) {
		// Set web view transparency

		NSString *name = GetSParam(cmd, 0);

		UWKWebView *view = [self getWebView:name];

		[view setTransparency:(float)cmd->iParams[0] / 100.0f];
	} else if(!strncmp(cmd->fourcc, "CLCK", 4)) {
		// Clear cookies (app level)
		NSHTTPCookie *cookie;
		NSHTTPCookieStorage *storage = [NSHTTPCookieStorage sharedHTTPCookieStorage];
		for(cookie in [storage cookies]) {
			[storage deleteCookie:cookie];
		}
	} else if(!strncmp(cmd->fourcc, "HFWD", 4)) {
		// navigate forward in web histoty

		NSString *name = GetSParam(cmd, 0);

		UWKWebView *view = [self getWebView:name];
		[[view webView] goForward];
	} else if(!strncmp(cmd->fourcc, "HBAK", 4)) {
		// navigate backwards in web history

		NSString *name = GetSParam(cmd, 0);

		UWKWebView *view = [self getWebView:name];

		[[view webView] goBack];
	} else if(!strncmp(cmd->fourcc, "SVCK", 4)) {
		// save cookies, automatic on iOS
	} else if(!strncmp(cmd->fourcc, "LOAD", 4)) {
		// Load the given URL into the web view

		NSString *name = GetSParam(cmd, 0);

		UWKWebView *view = [self getWebView:name];

		// this needs to span (or preferably be moved over to use the copy/free buffers from the plugin
		NSString *url = GetSParam(cmd, 1);
		[view load:url ];
	} else if(!strncmp(cmd->fourcc, "HTML", 4)) {

		// Load the given HTML into the web view
        
		NSString *name = GetSParam(cmd, 0);
        
		UWKWebView *view = [self getWebView:name];
        
        int idx = cmd->iParams[0];
        int size = cmd->iParams[1];
        
        unsigned char* dataOut = (unsigned char* )malloc(size);
        if (CopyAndFree(idx, dataOut, size))
        {
        
            NSString *html = [[NSString alloc] initWithBytes:dataOut
                                                   length:size
                                                   encoding:NSUTF16LittleEndianStringEncoding];
            
            [view loadHTML: html];
            
            [html release];
        }
        
        free(dataOut);
        
	} else if(!strncmp(cmd->fourcc, "SACT", 4)) {
		// set a view active or inactive

		NSString *name = GetSParam(cmd, 0);

		UWKWebView *view = [self getWebView:name];

		view.active = cmd->iParams[0] == 1 ? true : false;
	} else if(!strncmp(cmd->fourcc, "MRCT", 4)) {
		// set the rect for the mobile web view

		NSString *name = GetSParam(cmd, 0);

		UWKWebView *view = [self getWebView:name];

		[view setRect:cmd->iParams[0] y:cmd->iParams[1] width:cmd->iParams[2]  height:cmd->iParams[3]];
	} else if(!strncmp(cmd->fourcc, "RSIZ", 4)) {
		// resize the web view

		NSString *name = GetSParam(cmd, 0);

		UWKWebView *view = [self getWebView:name];

		[view resize:cmd->iParams[0]  height:cmd->iParams[1]];
	} else if(!strncmp(cmd->fourcc, "CAPT", 4)) {
		// capture the web page to a GLES texture

		NSString *name = GetSParam(cmd, 0);

		UWKWebView *view = [self getWebView:name];

		[view captureTexture:cmd->iParams[0] width:cmd->iParams[1] height:cmd->iParams[2]];
	} else if (!strncmp(cmd->fourcc, "EVJS", 4))
    {
        
		NSString *name = GetSParam(cmd, 0);
        
		UWKWebView *view = [self getWebView:name];
        
        int idx = cmd->iParams[0];
        int size = cmd->iParams[1];
        
        unsigned char* dataOut = (unsigned char* )malloc(size + 2);
        memset(dataOut, 0, size + 2);
        if (CopyAndFree(idx, dataOut, size))
        {
            
            NSString *jscript = [[NSString alloc] initWithBytes:dataOut
                                                  length:size 
                                                  encoding:NSUTF16LittleEndianStringEncoding];
            
            NSString *jscriptUTF8 = [[NSString alloc] initWithUTF8String: [jscript UTF8String]];
            
            NSString* result = [view evaluateJavascript: jscriptUTF8];

            if (result != nil)
            {
                
                NSData *bytes = [result dataUsingEncoding:NSUTF16LittleEndianStringEncoding];
                
                unsigned char* stringOut = (unsigned char* )malloc(bytes.length + 2);
                memset(stringOut, 0, bytes.length + 2);
                memcpy(stringOut, bytes.bytes, bytes.length);
            
                idx = AllocateAndCopy(stringOut, bytes.length + 2);
                if (idx != -1)
                {
                    cmd->retCode = 1;
                    cmd->iParams[0] = idx;
                    cmd->iParams[1] = bytes.length + 2;
                    PostCommand(cmd);
                }
                
                free(stringOut);
            }
            
            [jscript release];
            [jscriptUTF8 release];
        }
        
        free(dataOut);
        
        
    }
}

// Keyboard observation, we need to know when the keyboard is visible
// so that we don't push it back in the window hierarchy

- (void)noticeShowKeyboard:(NSNotification *)inNotification {
	keyboardVisible = true;

	/*
	 *
	 * // reference code for getting keyboard
	 *
	 * //The UIWindow that contains the keyboard view
	 * for(int j = 0; j < [[[UIApplication sharedApplication] windows] count]; j++)
	 * {
	 *  UIWindow* tempWindow = [[[UIApplication sharedApplication] windows] objectAtIndex:j];
	 *
	 *  //Because we cant get access to the UIKeyboard through the SDK we will just use UIView.
	 *  //UIKeyboard is a subclass of UIView anyways
	 *  UIView* keyboard;
	 *
	 *  //Iterate though each view inside of the selected Window
	 *  for(int i = 0; i < [tempWindow.subviews count]; i++)
	 *  {
	 *      //Get a reference of the current view
	 *      keyboard = [tempWindow.subviews objectAtIndex:i];
	 *
	 *      //Check to see if the className of the view we have referenced is "UIKeyboard" if so then we found
	 *      //the keyboard view that we were looking for
	 *      if([[keyboard description] hasPrefix:@"<UIKeyboard"] == YES)
	 *      {
	 *          //if (tempWindow.keyWindow)
	 *          //    printf_console("Got the keyboard\n");
	 *      }
	 *  }
	 * }
	 */
}

- (void)noticeHideKeyboard:(NSNotification *)inNotification {
	keyboardVisible = false;
}

+ (UWKWebEngine *)sInstance 
{
	static UWKWebEngine *instance;

	@synchronized(self) {
		if(!instance) {
			instance = [UWKWebEngine alloc];
			[instance init];
		}

		return instance;
	}

	return instance;
}

- (UWKWebEngine *)init 
{

	appController = GetAppController();
	
	viewLookup = [[NSMutableDictionary alloc] init];

	// We aren't fully initialized until the managed side has received
	// and processed the BOOT command
	initialized = false;


	// Setup the keyboard observer

	keyboardVisible = false;

	NSNotificationCenter *center = [NSNotificationCenter defaultCenter];
	[center addObserver:self selector:@selector(noticeShowKeyboard:) name:UIKeyboardDidShowNotification object:nil];
	[center addObserver:self selector:@selector(noticeHideKeyboard:) name:UIKeyboardWillHideNotification object:nil];


	return self;
}

@end

