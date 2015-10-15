This CSS style files contain fixed Sitecore 8 CSS styles in order to display it in more user-friendly manner without those huge paddings and monstrous elements coming with Sitecore 8. 
They don't look nice, don't fit small screens, especially those most of laptops have.

As all these files are CSS - they would not hurt your instance in any way. 
However I recommend to create a back-up of \sitecore\shell\Themes\Standard\Default folder, just in case you would need to return to original look-and-feel.

Build installation:
	1. Change [project properties] --> Build --> OutputPath to you Sitecore instance's \bin folder, and build project.
	2. Post build event command line will copy and replace original styles with those included int o this project.

Manual installation:
	Just copy all those files from project's Sitecore folder into your Sitecore 8 instance web root and agree to replace files.
	

Please read a blog post with more details and screenshots of adjustments:
	http://blog.martinmiles.net/post/fixing-ugly-default-sitecore-8-styles-from-huge-elements-padding-spacings-and-few-more-improvements


Hope you find this helpful!