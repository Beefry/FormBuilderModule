var gulp = require('gulp');
var sass = require('gulp-sass');
var concat = require('gulp-concat');

gulp.task('sass', function() {
	return gulp.src('css/sass/style.sass')
		.pipe(sass())
		.pipe(gulp.dest('css'));
});

gulp.task('concat', function(){
	gulp.src([
		'js/angular.min.js',
		'js/angular-resource.min.js',
		'angularjs/sortable.js',
		'angularjs/main.js',
		'angularjs/**/*Module.js',
		'angularjs/**/*Models.js',
		'angularjs/**/*Routes.js',
		'angularjs/**/*Services.js',
		'angularjs/**/*Directives.js',
		'angularjs/**/*Controllers.js',
	])
	.pipe(concat('app.js'))
	.pipe(gulp.dest('js'));
});

gulp.task('watch', function() {
	gulp.watch('angularjs/**/*.js',['concat']);
	gulp.watch('sass/*.sass',['sass']);
});