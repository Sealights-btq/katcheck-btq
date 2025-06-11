const { defineConfig } = require('cypress');
const { registerSealightsTasks } = require('sealights-cypress-plugin');

module.exports = defineConfig({
  e2e: {
    experimentalInteractiveRunEvents: true,
    testIsolation: false,
    async setupNodeEvents(on, config) {
      await registerSealightsTasks(on, config);
     },
  },
  // Disable screenshots and videos
  video: false,
  screenshotOnRunFailure: false
});
