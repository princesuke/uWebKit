/******************************************
  * uWebKit 
  * (c) 2013 The Engine Co. LLC
  * sales@uwebkit.com
*******************************************/

#import "UWKWebEngine.h"

@implementation AppController (UWKAppController)

- (void)applicationDidFinishLaunching:(UIApplication *)application {
	printf_console("-> UWK applicationDidFinishLaunching()\n");

	if([UIDevice currentDevice].generatesDeviceOrientationNotifications == NO)
		[[UIDevice currentDevice] beginGeneratingDeviceOrientationNotifications];

	[self startUnity:application];

	UWKWebEngine.sInstance.appController = self;
}

@end
