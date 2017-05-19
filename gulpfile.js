'use strict';

const $gulp = require('gulp');
const $gmsbuild = require('gulp-msbuild');
const $del = require('del');

const project = {
	paths: {
		dist: {
			root: './dist',
			resources: './dist/resources'
		},
		src: {
			configs: './src/configs/**/*',
			resources: './src/resources/**/*',
			csharp: {
				sln: './src/gtmp.evilempire.sln'
			}
		}
	},
	msbuild: {
		verbosity: 'minimal',
		stdout: true,
		toolsVersion: 14.0
	}
}

$gulp.task('default', ['rebuild']);

$gulp.task('rebuild', ['clean', 'build']);

$gulp.task('build', ['cs', 'copy']);

$gulp.task('copy', ['copy-resources', 'copy-settings']);

$gulp.task('clean', ['cs-clean'], function() {
	return $del([project.paths.dist.root]);
});

$gulp.task('cs-clean', function() {
	let msbuildConfig = Object.assign({ targets: ['Clean'] }, project.msbuild);
	return $gulp.src(project.paths.src.csharp.sln)
		.pipe($gmsbuild(msbuildConfig));
});

$gulp.task('copy-resources', function() {
	return $gulp.src(project.paths.src.resources)
		.pipe($gulp.dest(project.paths.dist.resources));
});

$gulp.task('copy-settings', function() {
	return $gulp.src(project.paths.src.configs)
		.pipe($gulp.dest(project.paths.dist.root));
});

$gulp.task('cs', function() {
	let msbuildConfig = Object.assign({ targets: ['Build'] }, project.msbuild);
	return $gulp.src(project.paths.src.csharp.sln)
		.pipe($gmsbuild(msbuildConfig));
});