/******************************************
  * uWebKit 
  * (c) 2013 THUNDERBEAST GAMES, LLC
  * sales@uwebkit.com
*******************************************/

#import <QuartzCore/QuartzCore.h>
#import "UWKWebView.h"
#import "UWKWebEngine.h"

// If you are compiling under Unity40, make sure this is defined
#define UWK_UNITY_40

#ifdef UWK_UNITY_40
UIWindow* UnityGetMainWindow();
#define UNITY_WINDOW UnityGetMainWindow()
#else
extern UIWindow *_window;
#define UNITY_WINDOW _window
#endif

// The view controller, can be customized here if desired
@interface UWKViewController : UIViewController {}
@end

// We also define a UIWindow class which can be customized
@implementation UWKWindow

- (void)sendEvent:(UIEvent *)event {
	[super sendEvent:event];
}

@end

@implementation UWKWebView

// properties
@synthesize webWindow;
@synthesize webView;
@synthesize rootView;
@synthesize name;
@synthesize smartRects;
@synthesize active;
@synthesize shown;
@synthesize ready;


// Initialize the web view and allocate its Window/Root/and UIWebView
- (UWKWebView *)init:(NSString *)viewName width:(int)width height:(int)height smartRects:(bool)_smartRects {
	name = viewName;
    
	active = true;
	ready = false;
    
	smartRects = _smartRects;
    
	int swidth = UNITY_WINDOW.screen.bounds.size.width;
	int sheight = UNITY_WINDOW.screen.bounds.size.height;
    
	// landscape
	webWindow = [[UWKWindow alloc] initWithFrame:CGRectMake(0, 0, swidth, sheight)];
    
	webWindow.opaque = true;
	webWindow.alpha = 1.0;
    
	self.name = viewName;
	webView = [[UIWebView alloc] initWithFrame:CGRectMake(0, 0, width, height)];
    
	webView.delegate = self;
    
	// this should be controllable, for instance we don't want this on facebook login screen
	// actually, this behaves properly on iPad where there is no retina scaling
	// on iPod/iPhone the scale is making the Facebook login weird
	webView.scalesPageToFit = YES;
    
	rootView = [[UIView alloc] initWithFrame:CGRectMake(0, 0, width, height)];
    
	// rootView.userInteractionEnabled = FALSE;
    
	// keeps from bouncing
	[[webView.subviews objectAtIndex:0] setBounces:NO];
    
	// keeps from scroll (scroll will blow main thread)
	// [[webView.subviews objectAtIndex:0] setScrollEnabled:NO];
    
	UWKViewController *controller = [[UWKViewController alloc] init];
    
	controller.view = rootView;
    
    [webWindow setRootViewController:controller];
    
	[rootView addSubview:webView];
	//[webWindow addSubview:rootView];
    
	webWindow.hidden = true;
    
	// owned by window, so we release here
	[rootView release];
	[webView release];
    
	return self;
}

- (oneway void)release {
	[super release];
    
	// explicitly decouple views and release
    
	if(webView != nil) {
		webView.delegate = nil;
		[webView removeFromSuperview];
		webView = nil;
	}
    
	if(rootView != nil) {
		[rootView removeFromSuperview];
		rootView = nil;
	}
    
	if(webWindow != nil) {
		[webWindow release];
		webWindow = nil;
	}
}

// set the views screen position and dimensions
- (void)setRect:(int)x y:(int)y width:(int)width height:(int)height {
	CGFloat w = webView.bounds.size.width;
	CGFloat h = webView.bounds.size.height;
    
	if(UNITY_WINDOW.screen.bounds.size.width == 320 || UNITY_WINDOW.screen.bounds.size.width == 480) {
		x /= 2;
		y /= 2;
        
		width  /= 2;
		height  /= 2;
	}
    
	CGFloat sx = ((float)width) / w;
	CGFloat sy = ((float)height) / h;
    
	CGAffineTransform tr = CGAffineTransformTranslate(CGAffineTransformIdentity, x, y);
	tr = CGAffineTransformScale(tr, sx, sy);
	webView.transform = tr;
    
	webView.center = CGPointMake((w * sx) - (w * sx / 2), h * sy / 2);
    
	ready = true;
}

// resize the view's bounds
- (void)resize:(int)width height:(int)height {
	CGAffineTransform tr = webView.transform;
    
	webView.bounds = CGRectMake(0, 0, width, height);
	webView.transform = tr;
}

- (void)update {
	// we're hidden until we have received our first SetRect
	if(!ready) {
		webWindow.hidden = true;
		return;
	}
    
	// If the keyboard is visible, get out of here
	if(UWKWebEngine.sInstance.keyboardVisible) {
		return;
	}
    
	if(shown && webWindow.keyWindow == NO) {
		// we're shown, so we're always the key window
		[webWindow makeKeyAndVisible];
	} else if(!shown && !webWindow.hidden) {
		// we're hidden
		webWindow.hidden = YES;
	}
}

// set the transparency level of the webview
- (void)setTransparency:(float)v;
{
	if(v < 0.0f)
		v = 0.0f;
	if(v > 1.0f)
		v = 1.0f;
    
    
	if(v == 1.0f) {
		webWindow.opaque = true;
		webWindow.alpha = 1.0;
		return;
	}
    
	webWindow.opaque = false;
	webWindow.alpha = v;
}

// toggle the visibility of the webview, the actual logic is in update
- (void)show:(bool)v;
{
	shown = v;
}


// once  the page starts loading, we will receive this signal
- (void)webViewDidStartLoad:(UIWebView *)wView {
	Command cmd;
    
	UWKWebView *uwk = (UWKWebView *)wView.delegate;
    
	InitCommand(&cmd, "PROG");
	SetSParam(&cmd, 0, uwk.name);
	cmd.numSParams = 1;
    
	// on iOS we don't get progress updates
	// so we just mark 50% progress, can use this to
	// update a throbber, etc in GUI
	cmd.iParams[0] = 50;
	cmd.numIParams = 1;
	PostCommand(&cmd);
}

// oops, the web page load failed
- (void)webView:(UIWebView *)wView didFailLoadWithError:(NSError *)error {
	// this can happen if the user clicks links quickly
	if(error.code == NSURLErrorCancelled) return;
    
	NSString *errorString = [error localizedDescription];
	NSString *errorTitle = [NSString stringWithFormat:@"Error"];
    
	UIAlertView *errorView = [[UIAlertView alloc] initWithTitle:errorTitle message:errorString delegate:self cancelButtonTitle:nil otherButtonTitles:@"OK", nil];
	[errorView show];
	[errorView autorelease];
    
	Command cmd;
	UWKWebView *uwk = (UWKWebView *)wView;
	InitCommand(&cmd, "LDFN");
	SetSParam(&cmd, 0, uwk.name);
	cmd.numSParams = 1;
	PostCommand(&cmd);
}

- (NSString *) evaluateJavascript:(NSString *) javascript {
    
    return [webView stringByEvaluatingJavaScriptFromString:javascript];
    
}

// the page finished loading, so tell Unity about it
- (void)webViewDidFinishLoad:(UIWebView *)wView;
{
	// NSString * jsCallBack = @"window.getSelection().removeAllRanges();";
	// [webView stringByEvaluatingJavaScriptFromString:jsCallBack];
    
	UWKWebView *uwk = (UWKWebView *)wView.delegate;
    
	Command cmd;
    
	// handle the title change
	NSString *title = [wView stringByEvaluatingJavaScriptFromString:@"document.title"];
    
	InitCommand(&cmd, "TITC");
	SetSParam(&cmd, 0, uwk.name);
	SetSParam(&cmd, 1, title);
	cmd.numSParams = 2;
	PostCommand(&cmd);
    
	// get the new url, which can change due to redirects and the like
	NSURL *url = wView.request.URL;
	if(url != nil) {
		NSData *bytes = [[url absoluteString ] dataUsingEncoding:NSUTF16LittleEndianStringEncoding];
		// [bytes release];
        
		InitCommand(&cmd, "URLC");
		SetSParam(&cmd, 0, uwk.name);
		cmd.numSParams = 1;
        
		int idx = AllocateAndCopy((unsigned char *)bytes.bytes, bytes.length);
        
		cmd.iParams[0] = idx;
		cmd.iParams[1] = bytes.length;
		cmd.numIParams = 2;
        
        
		PostCommand(&cmd);
	}
    
	// finally, tell the managed side that the page has finished loading
	InitCommand(&cmd, "LDFN");
	SetSParam(&cmd, 0, uwk.name);
	cmd.numSParams = 1;
	PostCommand(&cmd);
}

// load the given URL
- (void)loadHTML:(NSString *)html {
	// URL Object
	NSURL *nurl = [NSURL URLWithString:nil];
    
	// Load the request in the UIWebView.
	[webView loadHTMLString:html baseURL:nurl];
}

// load the given URL
- (void)load:(NSString *)url {
	// URL Request Object
	NSURL *nurl = [NSURL URLWithString:url];
	NSURLRequest *requestObj = [NSURLRequest requestWithURL:nurl];
    
	// Load the request in the UIWebView.
	[webView loadRequest:requestObj];
}

// capture a texture from the current loaded web page for use in GLES
// this can be quite slow for 1024x1024 pages, consider 512x512
// not really suitable for streaming video, etc.  There are better
// ways of handling these cases
- (void)captureTexture:(int)textureId width:(int)width height:(int)height {
	UIGraphicsBeginImageContext(webView.bounds.size);
	[webView.layer renderInContext:UIGraphicsGetCurrentContext()];
	UIImage *image = UIGraphicsGetImageFromCurrentImageContext();
	UIGraphicsEndImageContext();
    
	glBindTexture(GL_TEXTURE_2D, textureId);
    
	CGColorSpaceRef colorSpace = CGColorSpaceCreateDeviceRGB();
    
	// Allocate memory for image
	void *imageData = malloc(height * width * 4);
	memset(imageData, 255, height * width * 4);
    
	CGContextRef imgcontext = CGBitmapContextCreate(imageData, width, height, 8, 4 * width, colorSpace, kCGImageAlphaPremultipliedLast | kCGBitmapByteOrder32Big);
	CGColorSpaceRelease(colorSpace);
	CGContextClearRect(imgcontext, CGRectMake(0, 0, width, height) );
    
	CGContextTranslateCTM(imgcontext, 0, height);
	CGContextScaleCTM(imgcontext, 1, -1);
	CGContextDrawImage(imgcontext, CGRectMake(0, 0, width, height), image.CGImage);
    
	// Generate texture in opengl
	glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, width, height, 0, GL_RGBA, GL_UNSIGNED_BYTE, imageData);
    
	// release
	CGContextRelease(imgcontext);
	free(imageData);
}

@end

// Our View Controller forwards touches to its subviews
@implementation UWKViewController

- (BOOL)shouldAutorotateToInterfaceOrientation:(UIInterfaceOrientation)interfaceOrientation {
	return true;
}

// For ios6, use supportedInterfaceOrientations & shouldAutorotate instead
- (NSUInteger) supportedInterfaceOrientations{
    return UIInterfaceOrientationMaskAll;
}

- (BOOL) shouldAutorotate {
    return TRUE;
}

- (void)touchesBegan:(NSSet *)touches withEvent:(UIEvent *)event {
	[[UNITY_WINDOW.subviews objectAtIndex:0] touchesBegan:touches withEvent:event];
}

- (void)touchesCancelled:(NSSet *)touches withEvent:(UIEvent *)event {
	[[UNITY_WINDOW.subviews objectAtIndex:0] touchesCancelled:touches withEvent:event];
}

- (void)touchesEnded:(NSSet *)touches withEvent:(UIEvent *)event {
	[[UNITY_WINDOW.subviews objectAtIndex:0] touchesEnded:touches withEvent:event];
}

- (void)touchesMoved:(NSSet *)touches withEvent:(UIEvent *)event {
	[[UNITY_WINDOW.subviews objectAtIndex:0] touchesMoved:touches withEvent:event];
}

@end


