'use strict';
let $sha = require('./src/public/libs/client/sha512');
let msg = process.argv[process.argv.length - 1];
console.log($sha.cshash(msg));