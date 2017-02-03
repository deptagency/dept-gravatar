// var sharp = require('sharp'); // A popular image manipulation package on NPM
var Jimp = require("jimp");
var ContentStream = require('contentstream');

module.exports = function (callback, physicalPath, destinationPath, maxWidth, maxHeight) {


    Jimp.read(physicalPath).then(function (image) {
        image.resize(maxWidth, maxHeight)
         .write(destinationPath); // save
        callback(/* error */ null, true);
         
        }).catch(function (err) {
            callback(err, false);
        });
}