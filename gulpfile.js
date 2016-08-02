var gulp = require("gulp");
var shell = require("gulp-shell");
var args = require('yargs').argv;
var debug = require('gulp-debug');

var config = {
  name: 'NScientist',
  mode: args.mode || "Debug",
  output: "./build/deploy"
}

gulp.task("default", [ "restore", "compile", "test", "pack" ]);

gulp.task('restore', shell.task([
  'dotnet restore'
]));

gulp.task('compile', [ "restore" ], shell.task([
  `dotnet build ./src/${config.name} --configuration ${config.mode}`,
  `dotnet build ./src/${config.name}.Tests --configuration ${config.mode}`
]));

gulp.task('test', [ "compile" ], shell.task([
  `dotnet test ./src/${config.name}.Tests --configuration ${config.mode}`
]));

gulp.task('pack', [ 'test' ], shell.task([
  `dotnet pack ./src/${config.name} --configuration ${config.mode} --output ${config.output}`
]));
