const path = require('path');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const CopyWebpackPlugin = require('copy-webpack-plugin');

module.exports = {
  mode: 'development',
  devtool: 'eval-source-map',
  entry: './src/main.ts',
  output: {
    path: path.resolve(__dirname, '../dist'),
    filename: 'bundle.js',
    clean: true,
  },
  resolve: {
    extensions: ['.ts', '.js'],
    alias: {
      '@core': path.resolve(__dirname, '../src/core'),
      '@entities': path.resolve(__dirname, '../src/entities'),
      '@systems': path.resolve(__dirname, '../src/systems'),
      '@data': path.resolve(__dirname, '../src/data'),
      '@scenes': path.resolve(__dirname, '../src/scenes'),
      '@ui': path.resolve(__dirname, '../src/ui'),
      '@utils': path.resolve(__dirname, '../src/utils'),
      '@shaders': path.resolve(__dirname, '../src/shaders'),
    },
  },
  module: {
    rules: [
      {
        test: /\.ts$/,
        use: 'ts-loader',
        exclude: /node_modules/,
      },
      {
        test: /\.(glsl|vert|frag)$/,
        use: 'raw-loader',
      },
    ],
  },
  plugins: [
    new HtmlWebpackPlugin({ template: './public/index.html' }),
    new CopyWebpackPlugin({
      patterns: [{ from: 'assets', to: 'assets' }],
    }),
  ],
  devServer: {
    static: path.resolve(__dirname, '../dist'),
    port: 8080,
    hot: true,
    open: false,
  },
};
