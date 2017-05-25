'use strict';
let $sha = require('js-sha512');
let msg = process.argv[process.argv.length - 1];
let a = '__' + msg + '::0';
for (let i = 0; i < 10; i++) {
    a = $sha.sha512(a);
}
msg = '::' + a;
console.log(msg);