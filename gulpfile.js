'use strict';

const $childproc = require('child_process');
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
            resources: './dist/resources',
            dbt: './dist/dbt',
            maps: './dist/maps'
        },
        src: {
            configs: './src/configs/**/*',
            resources: './src/resources/**/*',
            public: './src/public/**/!(*.tests.*)',
            csharp: {
                sln: './src/gtmp.evilempire.sln',
                binaries: [ './src/gtmp.evilempire.db/bin/debug/*', './src/gtmp.evilempire.server/bin/debug/*', './src/gtmp.evilempire.server.launcher/bin/debug/*', './src/gtmp.evilempire.shared/bin/debug/*' ]
            },
            dbt: './src/db/templates/*',
            maps: './src/maps/*'
        },
        libs: {
            gtmp: {
                serverzip: './libs/gtmp/v0.1.513.481/GT-MP-Server.zip'
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
    transformToGtmpResource: function(elements) {
        let stream = new $stream.Transform({
            objectMode: true,
            transform: function(f, enc, cb) {
                if (f.isDirectory()) {
                    return cb(null, null);
                }
                if (f.path.endsWith('.client.js') ||
                    f.path.endsWith('.js') && f.path.match(/[/\\]libs[/\\]client[/\\]/)) {
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
        Object.getOwnPropertyNames(elements).forEach(elementName => {
            stream.contents += `<${elementName} `;
            let element = elements[elementName];
            Object.getOwnPropertyNames(element).forEach(attributeName => {
                stream.contents += `${attributeName}="${element[attributeName]}" `;
            });
            stream.contents += ' />'
        });

        return stream;
    }
};

$gulp.task('default', ['rebuild']);

$gulp.task('run', function(cb) {
    try {
        $childproc.execSync('taskkill /im grandtheftmultiplayer.server.exe');
    } catch(ex) {
    }
    $childproc.exec(__dirname + '/dist/gtmp.evilempire.server.launcher.exe', { cwd: __dirname +'/dist/' });
    cb();
});

$gulp.task('rebuild', function(cb) {
    $runseq('clean', 'build', cb);
});

$gulp.task('build-resources', ['copy-public', 'build-resources-meta']);

$gulp.task('build', function(cb) {
    $runseq('cs', 'copy', 'build-resources', 'dist', cb);
});

$gulp.task('build-cs', function(cb) {
    $runseq('cs', 'copy-cs', cb);
});

$gulp.task('copy', function(cb) {
    $runseq('extract-server', 'delete-resources', 'copy-cs', ['copy-public', 'copy-settings', 'copy-dbt', 'copy-maps'], cb);
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

$gulp.task('copy-public', function() {
    return $gulp.src(project.paths.src.public)
        .pipe($gulp.dest(project.paths.dist.resources + '/gtmp-server'));
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

$gulp.task('copy-dbt', function() {
    return $gulp.src(project.paths.src.dbt)
        .pipe($gflat())
        .pipe($gulp.dest(project.paths.dist.dbt));
});

$gulp.task('copy-maps', function() {
    return $gulp.src(project.paths.src.maps)
        .pipe($gflat())
        .pipe($gulp.dest(project.paths.dist.maps));
});

$gulp.task('build-resources-meta', function() {
    return $gulp.src(project.paths.src.public)
        .pipe($__tasks.transformToGtmpResource({
            info: { name: 'GTMP Evil Empire Server', author: 'lloyd', type: 'script' },
            script: { src: './../../gtmp.evilempire.server.dll', type: 'server', lang: 'compiled' }
        })).pipe($gulp.dest(project.paths.dist.root + '/resources/gtmp-server'));
});

$gulp.task('extract-server', function() {
    return $gulp.src(project.paths.libs.gtmp.serverzip)
        .pipe($gdecompress())
        .pipe($gulp.dest(project.paths.dist.root));
});

$gulp.task('cs', function() {
    let msbuildConfig = Object.assign({ targets: ['Rebuild'] }, project.msbuild);
    return $gulp.src(project.paths.src.csharp.sln)
        .pipe($gmsbuild(msbuildConfig));
});

$gulp.task('dist', function() {
    return $gulp.src([ project.paths.dist.root + '**/!(settings.user.xml|users.xml)' ])
        .pipe($gzip('server.zip'))
        .pipe($gulp.dest(project.paths.dist.root));
});
