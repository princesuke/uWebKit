/******************************************
  * uWebKit 
  * (c) 2013 THUNDERBEAST GAMES, LLC
  * sales@uwebkit.com
*******************************************/

#import "UnityAppController.h"
#import "UWKPlugin.h"
#import "UWKWebView.h"

// The UWKWebEngine manages the table of allocated web views and forwards commands to them from
// the managed Unity side
@interface UWKWebEngine : NSObject {
	// The Unity App controller
	UnityAppController *appController;

	// Our webview lookup table
	NSMutableDictionary *viewLookup;

	// Whether the engine is initialized or not
	bool initialized;

	// If the touch keyboard is visible, this will be true
	bool keyboardVisible;
}

// The one and only web engine instance
+ (UWKWebEngine *)sInstance;

// initialize the web engine which includes the lookup table and setting up
// keyboard notifications
- (UWKWebEngine *)init;

// tick the engine and update the views
- (void)tick;

// process inbound commands from Unity
- (void)processCommand:(Command *)cmd;

// retrieve the web view with the given name
- (UWKWebView *)getWebView:(NSString *)name;

// add and allocate a new named web view
- (UWKWebView *)addWebView:(NSString *)name width:(int)width height:(int)height smartRects:(bool)smartRects;


// properties
@property (retain) UnityAppController *appController;

@property (retain) NSMutableDictionary *viewLookup;

@property bool initialized;

@property bool keyboardVisible;


@end
