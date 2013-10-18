/******************************************
  * uWebKit 
  * (c) 2013 THUNDERBEAST GAMES, LLC
  * sales@uwebkit.com
*******************************************/

@interface UWKWindow : UIWindow {
}

@end

@interface UWKWebView : NSObject<UIWebViewDelegate> {
	// Overlay window for webview
	UWKWindow *webWindow;

	// Native web view
	UIWebView *webView;

	// Root view controlled by the view controller
	UIView *rootView;

	// The name of this web view, used to communicate commands from managed Unity
	NSString *name;

	// Whether or not this view is set to use smart rects, this has no effect on iOS
	// It is here for reference
	bool smartRects;

	// Whether or not the view is active
	bool active;

	// A view is invisible until shown
	bool shown;

	// A web view is marked ready after its first SetRect (once the bounds are valid)
	bool ready;
}

// initialize and allocate the UWKWebView and associated window/root/native web view
- (UWKWebView *)init:(NSString *)viewName width:(int)width height:(int)height smartRects:(bool)_smartRects;

// capture a web view to a texture for use in GLES
- (void)captureTexture:(int)textureId width:(int)width height:(int)height;

// Load a given URL
- (void)load:(NSString *)url;

// Load a given HTML
- (void)loadHTML:(NSString *)html;

// Evaluate given JavaScript and return result
- (NSString *) evaluateJavascript:(NSString *) javascript;

// Set the web view's screen position and dimensions
- (void)setRect:(int)x y:(int)y width:(int)width height:(int)height;

// Resize the web view to the given dimension
- (void)resize:(int)width height:(int)height;

// Set the level of transparency on the view
- (void)setTransparency:(float)v;

// Show or Hide the view
- (void)show:(bool)v;

// Called to update the views key window status, etc
- (void)update;

// Property setup
@property (retain) UIWebView *webView;

@property (retain) NSString *name;

@property (retain) UIWindow *webWindow;

@property (retain) UIView *rootView;

@property bool smartRects;

@property bool active;

@property bool shown;

@property bool ready;

@end
