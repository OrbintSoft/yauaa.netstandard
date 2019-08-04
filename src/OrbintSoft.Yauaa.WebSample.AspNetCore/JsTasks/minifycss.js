'use strict';

const glob = require('glob');
const CleanCSS = require('clean-css');
const fs = require('fs');
var count = 0;

const globOptions = {
    realpath: true,
    nodir: true
};

const compile = function (input, outputPath) {
    console.log('START COMPILE: ' + input);   
    var cleanCSS = new CleanCSS({ returnPromise:true});
    fs.readFile(input, 'utf8', function read(err, data) {
        if (err) {
            console.dir(err);
            process.exit(-2);
        }            
        cleanCSS.minify(data).then(function(output) {
            fs.writeFile(outputPath, output.styles, function (err) {
                if (err) {
                    process.exit(-4);
                } else {
                    console.log('END COMPILE: ' + outputPath);
                }
                
            });
        }).catch(function (error) {
            console.dir(error);
            process.exit(-3);
        });
    });
};

glob("wwwroot/**/*.css", globOptions, function (er, files) {
    if (er) {
        console.dir(er);
        process.exit(-1);
    } else {        
        count = files.length;
        for (var i = 0; i < count; i++) {
            if (!files[i].endsWith('.min.css')) {
                const outputPath = files[i].substr(0, files[i].lastIndexOf('.css')) + '.min.css';
                if (!fs.existsSync(outputPath)) {
                    compile(files[i], outputPath);
                }
            }                     
        }
    }
});