/// <binding AfterBuild='babel, concat-uglify' />
'use strict';
const gulp = require('gulp');
const babel = require('gulp-babel');
const uglify = require('gulp-uglify');
const concat = require('gulp-concat');
const rename = require('gulp-rename');

gulp.task('default', ['babel', 'concat-uglify']);

gulp.task('babel', () => {

	// Use babeljs on the letmein javascript to transcode it from ECMAScript
	return gulp
		.src('js/letmein.js')
		.pipe(babel({
			presets: ['es2015']
		}))
		.pipe(gulp.dest('js/prod', { overwrite: true }));
});

gulp.task('concat-uglify', () => {
	return gulp
		.src(['js/libraries/jquery-3.1.1.min.js',
			'js/libraries/bootstrap.min.js',
			'js/libraries/bootbox.min.js',
			'js/libraries/clipboard.min.js',
			'js/libraries/jquery.countdown.min.js',
			'js/libraries/jquery.toast.js',
			'js/libraries/sjcl.js',
			'js/prod/letmein.js'
		])
		.pipe(concat('letmein.min.js'))
		.pipe(uglify())
		.pipe(gulp.dest('js/prod'));
});