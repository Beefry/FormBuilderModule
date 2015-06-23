-In order to get this to work, you'll need to run this on a web server because of the XHR request to get the Builder.htm template file for the AngularJS directive (or to get any template file really). In a later release I might include this HTML in the Angular javascript file to cut down on files needed to inject, but for now this is how it's being done.

-Also, a Template directory needs to be duplicated on the root of the web directory since this is how the Angular app references those template files.

-Run "npm install" in order to install all the gulp-related modules needed for the compilation of the Angular app.

-Once Gulp is installed from the directory, run "gulp watch" in the command line from the directory where the gulpfile.js file is located.