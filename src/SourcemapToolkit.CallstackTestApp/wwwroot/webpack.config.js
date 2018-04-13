const path = require('path');

module.exports = {
    mode: "development",
    entry: './out/index.js',
    devtool:
        //  "source-map"
        "nosources-source-map"
    ,
    output: {
        path: path.resolve(__dirname, 'dist'),
        filename: 'bundle.js',
        // publicPath: "/dist",
    },
    module: {
        rules: [
            {
                test: /\.js$/,
                use: ["source-map-loader"],
                enforce: "pre"
            }
        ]
    }
};
