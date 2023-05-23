// package: sudo npm install -g pkg
// pkg fswatch.js --target=node18-macos-arm64
const fs = require('fs')
const args = process.argv.slice(2);
const filePath = args[0] 
fs.watch(filePath, (eventType, filename) => {
    console.log(eventType);
});