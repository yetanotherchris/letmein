/// <binding AfterBuild='babel, concat-uglify' />
'use strict';
const gulp = require('gulp');
const babel = require('gulp-babel');
const uglify = require('gulp-uglify');
const concat = require('gulp-concat');
const rename = require('gulp-rename');

gulp.task('default', ['babel', 'concat-uglify']);

gulp.task('babel', () => {
	return gulp
		.src('js/letmein.js')
		.pipe(babel({
			presets: ['es2015']
		}))
		.pipe(gulp.dest('js/prod', { overwrite: true }));
});

gulp.task('concat-uglify', () => {
	return gulp
		.src(['js/prod/letmein.js'])
		.pipe(concat('js/libraries/*.js'))
		.pipe(uglify())
		.pipe(rename('letmein.min.js'))
		.pipe(gulp.dest('js/prod'));
});