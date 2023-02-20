// Template for webpack.config.js in Fable projects
// Find latest version in https://github.com/fable-compiler/webpack-config-template

// In most cases, you'll only need to edit the CONFIG object (after dependencies)
// See below if you need better fine-tuning of Webpack options

// Dependencies. Also required: sass, sass-loader, css-loader, style-loader, file-loader, resolve-url-loader
var path = require('path');
var webpack = require('webpack');
var HtmlWebpackPlugin = require('html-webpack-plugin');

var CONFIG = {
    // The tags to include the generated JS and CSS will be automatically injected in the HTML template
    // See https://github.com/jantimon/html-webpack-plugin
    indexHtmlTemplate: './src/index.html',
    fsharpEntry: './src/App.fs.js',
    outputDir: './dist',
    assetsDir: './public',
    publicPath: '/', // Where the bundled files are accessible relative to server root
    devServerPort: 8080,
    // When using webpack-dev-server, you may need to redirect some calls
    // to a external API server. See https://webpack.js.org/configuration/dev-server/#devserver-proxy
    devServerProxy: undefined,
}

// If we're running webpack serve, assume we're in development
var isProduction = !hasArg(/serve/);
var outputWebpackStatsAsJson = hasArg('--json');

if (!outputWebpackStatsAsJson) {
    console.log("Bundling CLIENT for " + (isProduction ? "production" : "development") + "...");
}

// The HtmlWebpackPlugin allows us to use a template for the index.html page
// and automatically injects <script> or <link> tags for generated bundles.
var commonPlugins = [
    new HtmlWebpackPlugin({
        filename: 'index.html',
        template: resolve(CONFIG.indexHtmlTemplate)
    })
];

module.exports = {
    entry: {
        app: [resolve(CONFIG.fsharpEntry)]
    } ,
    // Add a hash to the output file name in production
    // to prevent browser caching if code changes
    output: {
        publicPath: CONFIG.publicPath,        
        path: resolve(CONFIG.outputDir),
        filename: isProduction ? '[name].[contenthash].js' : '[name].js',
    },
    mode: isProduction ? 'production' : 'development',
    devtool: isProduction ? 'source-map' : 'eval-source-map',
    optimization: {
        splitChunks: {
            chunks: 'all'
        },
    },

    plugins: commonPlugins,
    // Configuration for webpack-dev-server
    devServer: {
        // Necessary when using non-hash client-side routing
        // This assumes the index.html is accessible from server root
        // For more info, see https://webpack.js.org/configuration/dev-server/#devserverhistoryapifallback
        historyApiFallback: {
            index: '/'
        },
        host: '0.0.0.0',
        port: CONFIG.devServerPort,
        proxy: CONFIG.devServerProxy,
        hot: true,
    },
    // - sass-loaders: transforms SASS/SCSS into JS
    // - file-loader: Moves files referenced in the code (fonts, images) into output folder
    module: {
        rules: [
            {
                test: /\.(png|jpg|jpeg|gif|svg|woff|woff2|ttf|eot)(\?.*)?$/,
                use: ['file-loader']
            }
        ]
    }
};

function resolve(filePath) {
    return path.isAbsolute(filePath) ? filePath : path.join(__dirname, filePath);
}

function hasArg(arg) {
    return arg instanceof RegExp
        ? process.argv.some(x => arg.test(x))
        : process.argv.indexOf(arg) !== -1;
}