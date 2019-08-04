// Cleans the output [distribution] files
const fs = require("fs");
const glob = require("glob");

const options = {
    realpath: true,
    nodir: true
};

glob("wwwroot/**/*.js.map", options, function (er, files) {
    console.log('START CLEAN: .js.map FILES');
    remove(er, files);
    console.log('END CLEAN: .js.map FILES');
});

glob("wwwroot/**/*.min.js", options, function (er, files) {
    console.log('START CLEAN: .min.js FILES');
    remove(er, files);
    console.log('END CLEAN: .min.js FILES');
});

glob("wwwroot/**/*.min.css", options, function (er, files) {
    console.log('START CLEAN: .min.css FILES');
    remove(er, files);
    console.log('END CLEAN: .min.css FILES');
});

function remove(er, files) {
    if (er) {
        console.dir(er);
        process.exit(-1);
    }
    else {
        for (var i = 0; i < files.length; i++) {            
            if (fs.existsSync(files[i])) {
                fs.unlinkSync(files[i]);
            }
            console.log('DELETE: ' + files[i]);
        }
    }
}
