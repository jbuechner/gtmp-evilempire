'use strict';

const $gulp = require('gulp');
const $gflat = require('gulp-flatten');
const $gdecompress = require('gulp-decompress');
const $gzip = require('gulp-zip');
const $runseq = require('run-sequence');
const $gmsbuild = require('gulp-msbuild');
const $del = require('del');

const project = {
	paths: {
		dist: {
			root: './dist/',
			resources: './dist/resources'
		},
		src: {
			configs: './src/configs/**/*',
			resources: './src/resources/**/*',
			csharp: {
				sln: './src/gtmp.evilempire.sln',
				binaries: './src/gtmp.evilempire*/bin/debug/*'
			}
		},
		libs: {
			gtmp: {
				serverzip: './libs/gtmp/v0.1.501.566/GT-MP-Server.zip'
			}
		}
	},
	msbuild: {
		verbosity: 'minimal',
		stdout: true,
		toolsVersion: 14.0,
		properties: {
			Configuration: 'Debug'
		}
	}
}

$gulp.task('default', ['rebuild']);

$gulp.task('rebuild', function(cb) {
	$runseq('clean', 'build', cb);
});

$gulp.task('build', function(cb) {
	$runseq('cs', 'copy', 'dist', cb);
});

$gulp.task('copy', function(cb) {
	$runseq('extract-server', 'delete-resources', ['copy-cs', 'copy-resources', 'copy-settings'], cb)
});

$gulp.task('clean', ['cs-clean'], function() {
	return $del([project.paths.dist.root]);
});

$gulp.task('delete-resources', function() {
	return $del([project.paths.dist.root + 'resources']);
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

$gulp.task('copy-cs', function() {
	return $gulp.src(project.paths.src.csharp.binaries)
		.pipe($gflat())
		.pipe($gulp.dest(project.paths.dist.root));
});


$gulp.task('extract-server', function() {
	return $gulp.src(project.paths.libs.gtmp.serverzip)
		.pipe($gdecompress())
		.pipe($gulp.dest(project.paths.dist.root));
});

$gulp.task('cs', function() {
	let msbuildConfig = Object.assign({ targets: ['Build'] }, project.msbuild);
	return $gulp.src(project.paths.src.csharp.sln)
		.pipe($gmsbuild(msbuildConfig));
});

$gulp.task('dist', function() {
	return $gulp.src([ project.paths.dist.root + '**/!(settings.user.xml)' ])
		.pipe($gzip('server.zip'))
		.pipe($gulp.dest(project.paths.dist.root));
});