'use strict';

const $gulp = require('gulp');
const $del = require('del');

const project = {
	paths: {
		dist: {
			root: './dist',
			resources: './dist/resources'
		},
		src: {
			resources: './src/resources/**/*'
		}
	}
}

$gulp.task('default', ['rebuild']);

$gulp.task('rebuild', ['clean', 'build']);

$gulp.task('build', ['copy']);

$gulp.task('copy', ['copy-resources']);

$gulp.task('clean', function() {
	return $del([project.paths.dist.root]);
});

$gulp.task('copy-resources', function() {
	return $gulp.src(project.paths.src.resources)
		.pipe($gulp.dest(project.paths.dist.resources));
});