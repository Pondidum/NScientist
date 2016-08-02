var gulp = require("gulp");
var shell = require("gulp-shell");
var args = require('yargs').argv;
var fs = require("fs");
var debug = require('gulp-debug');

var project = JSON.parse(fs.readFileSync("./package.json"));

var config = {
  name: project.name,
  mode: args.mode || "Debug",
  commit: process.env.APPVEYOR_REPO_COMMIT || "0",
  buildNumber: process.env.APPVEYOR_BUILD_VERSION || "0",
  output: "./build/deploy"
}

gulp.task("default", [ "restore", "compile", "test", "pack" ]);

gulp.task('restore', function() {
  return gulp
    .src(config.name + '.sln', { read: false })
    .pipe(shell('dotnet restore'));
});

gulp.task('compile', [ "restore" ], shell.task([
  'dotnet build ./src/' + config.name + ' --configuration ' + config.mode,
  'dotnet build ./src/' + config.name + '.Tests --configuration ' + config.mode
]));

gulp.task('test', [ "compile" ], shell.task([
  'dotnet test ./src/' + config.name + '.Tests --configuration ' + config.mode
]));

gulp.task('pack', [ 'test' ], shell.task([
  'dotnet pack ./src/' + config.name + ' --configuration ' + config.mode + ' --output ./build/deploy'
]));
