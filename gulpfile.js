var gulp = require("gulp");
var shell = require("gulp-shell");
var args = require('yargs').argv;
var fs = require("fs");
var assemblyInfo = require('gulp-dotnet-assembly-info');
var rename = require('gulp-rename');
var msbuild = require('gulp-msbuild');
var xunit =require('gulp-xunit-runner');
var debug = require('gulp-debug');

var project = JSON.parse(fs.readFileSync("./package.json"));

var config = {
  name: project.name,
  version: project.version,
  mode: args.mode || "Debug",
  commit: process.env.APPVEYOR_REPO_COMMIT || "0",
  buildNumber: process.env.APPVEYOR_BUILD_VERSION || "0",
  output: "./build/deploy"
}

gulp.task("default", [ "restore", "version", "compile", "test", "pack" ]);

gulp.task('restore', function() {
  return gulp
    .src(config.name + '.sln', { read: false })
    .pipe(shell('dotnet restore'));
});

gulp.task('version', function() {
  return gulp
    .src('./AssemblyVersion.base')
    .pipe(rename("AssemblyVersion.cs"))
    .pipe(assemblyInfo({
      version: config.version,
      fileVersion: config.version,
      description: "Build: " +  config.buildNumber + ", Sha: " + config.commit
    }))
    .pipe(gulp.dest('./src/' + config.name + '/Properties'));
});

gulp.task('compile', [ "restore", "version" ], shell.task([
  'dotnet build ./src/' + config.name + ' --configuration ' + config.mode,
  'dotnet build ./src/' + config.name + '.Tests --configuration ' + config.mode
]));

gulp.task('test', [ "compile" ], shell.task([
  'dotnet test ./src/' + config.name + '.Tests --configuration ' + config.mode
]));

gulp.task('pack', [ 'test' ], function () {
  return gulp
    .src('**/*.nuspec', { read: false })
    .pipe(rename({ extname: ".csproj" }))
    .pipe(shell([
      '"tools/nuget/nuget.exe" pack <%= file.path %> -version <%= version %> -prop configuration=<%= mode %> -o <%= output%>'
    ], {
      templateData: config
    }));
});
