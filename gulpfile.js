'use strict';

const $stream = require('stream');
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
			public: './src/public/**/*',
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

const $__tasks = {
	transformToGtmpResource: function(info) {
		let stream = new $stream.Transform({
			objectMode: true,
			transform: function(f, enc, cb) {
				if (f.path.endsWith('.js')) {
					this.contents += '<script src="' + f.relative + '" type="client" lang="javascript" />\n';
				} else {
					this.contents += '<file src="' + f.relative + '" />\n';
				}
				cb(null, null);
			},
			flush: function(cb) {
				this.contents += '</meta>';
				cb(null, { contents: this.contents, relative: 'meta.xml', isDirectory: () => false, isStream: () => false, isBuffer: () => true, isNull: () => false });
			}
		});
		stream.contents = '<meta>\n';
		stream.contents += '<info ';
		Object.getOwnPropertyNames(info).forEach(function(propertyName) {
			stream.contents += 'propertyName="'  + info[propertyName] + '" ';
		});
		stream.contents += '/>\n';
		return stream;
	}
};

$gulp.task('default', ['rebuild']);

$gulp.task('rebuild', function(cb) {
	$runseq('clean', 'build', cb);
});

$gulp.task('build', function(cb) {
	$runseq('cs', 'copy', 'build-resourcemeta', 'dist', cb);
});

$gulp.task('copy', function(cb) {
	$runseq('extract-server', 'delete-resources', ['copy-cs', 'copy-resources'], ['copy-public', 'copy-settings'], cb)
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

$gulp.task('copy-public', function() {
	return $gulp.src(project.paths.src.public)
		.pipe($gulp.dest(project.paths.dist.resources + '/gtmp-public'));
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

$gulp.task('build-resourcemeta', function() {
	return $gulp.src(project.paths.src.public)
		.pipe($__tasks.transformToGtmpResource({ name: 'GTMP Evil Empire Files' }))
		.pipe($gulp.dest(project.paths.dist.root + '/resources/gtmp-public'));
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