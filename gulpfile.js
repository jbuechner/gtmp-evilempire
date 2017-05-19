'use strict';

const $gulp = require('gulp');
const $del = require('del');

const project = {
	paths: {
		dist: './dist'
	}
}

$gulp.task('default', ['rebuild']);

$gulp.task('rebuild', ['clean']);

$gulp.task('clean', function() {
	return $del([project.paths.dist]);
});