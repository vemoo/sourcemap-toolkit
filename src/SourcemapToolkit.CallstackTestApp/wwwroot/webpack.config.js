const path = require('path');

const isProd = process.argv.indexOf("-p") >= 0;

module.exports = {
    mode: isProd ? "production" : "development",
    entry: './out/index.js',
    devtool: "source-map",
    output: {
        path: path.resolve(__dirname, 'dist'),
        filename: 'bundle.js',
    },
    module: {
        rules: [
            {
                test: /\.js$/,
                use: ["source-map-loader"],
                enforce: "pre"
            }
        ]
    },
    externals: {
        "jquery": "jQuery"
    }
};
