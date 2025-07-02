#\!/usr/bin/env node
const fs = require("fs");
const path = require("path");

// Simple SVG to PNG placeholder - creates solid color squares
const sizes = [
  { name: "icon-192x192.png", size: 192 },
  { name: "icon-512x512.png", size: 512 },
  { name: "apple-touch-icon.png", size: 180 }
];

const svg = (size) => `<svg xmlns="http://www.w3.org/2000/svg" width="${size}" height="${size}" viewBox="0 0 ${size} ${size}">
  <rect width="${size}" height="${size}" fill="#3b82f6"/>
  <circle cx="${size/2}" cy="${size/2}" r="${size/4}" fill="white"/>
</svg>`;

sizes.forEach(({ name, size }) => {
  fs.writeFileSync(path.join("public", name), Buffer.from(svg(size)));
});

console.log("Placeholder icons created");

