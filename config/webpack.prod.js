const path = require('path');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const CopyWebpackPlugin = require('copy-webpack-plugin');
const TerserPlugin = require('terser-webpack-plugin');

module.exports = {
  mode: 'production',
  devtool: 'source-map',
  entry: './src/main.ts',
  output: {
    path: path.resolve(__dirname, '../dist'),
    filename: '[name].[contenthash].js',
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
      { test: /\.ts$/, use: 'ts-loader', exclude: /node_modules/ },
    ],
  },
  optimization: {
    minimize: true,
    minimizer: [new TerserPlugin()],
    splitChunks: { chunks: 'all' },
  },
  plugins: [
    new HtmlWebpackPlugin({ template: './public/index.html', minify: true }),
    new CopyWebpackPlugin({ patterns: [{ from: 'assets', to: 'assets' }] }),
  ],
};
