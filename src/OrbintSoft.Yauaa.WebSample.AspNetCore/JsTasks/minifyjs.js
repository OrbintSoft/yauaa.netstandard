"use strict";

const glob = require('glob');
const fs = require('fs');
const ClosureCompiler = require('google-closure-compiler').compiler;
var count = 0;
var compiled = 0;

const options = {
    realpath: false,
    nodir: true
};

const compile = function (input, output) {
    console.log('START COMPILE: ' + input);    
    var closureCompiler = new ClosureCompiler({
        compilation_level: 'SIMPLE',
        create_source_map: output +'.map',
        env: 'BROWSER',
        js: input,
        js_output_file: output,
        language_in: 'STABLE',
        language_out: 'ECMASCRIPT_2015',
        warning_level: 'QUIET'
    });
    closureCompiler.run((exitCode, stdOut, stdErr) => {        
        if (stdErr) {
            console.dir(stdErr);
            process.exit(-1);
        }
        console.log('END COMPILE: ' + output);
        compiled++;
        if (compiled === count) {
            console.log('**********************');
            console.log('ALL JS FILES COMPILED!');
        }
    });
};

glob("wwwroot/**/*.js", options, function (er, files) {
    if (er) {
        console.dir(er);
        process.exit(-1);
    } else {        
        count = files.length;
        for (var i = 0; i < count; i++) {
            if (!files[i].endsWith('.min.js')) {
                const output = files[i].substr(0, files[i].lastIndexOf('.js')) + '.min.js';
                if (!fs.existsSync(output))
                {
                    compile(files[i], output);
                }                
            }                     
        }
    }
});