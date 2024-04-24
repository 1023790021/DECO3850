#include <Adafruit_CircuitPlayground.h>

const float PICK_UP_THRESHOLD = 1.0;
const float PUT_DOWN_THRESHOLD = 0.1;
bool isPickedUp = false;

void setup() {
  Serial.begin(9600);
  CircuitPlayground.begin();
}

void loop() {
  // Read acceleration data for x, y, and z axes
  float x = CircuitPlayground.motionX();
  float y = CircuitPlayground.motionY();
  float z = CircuitPlayground.motionZ();

  if (z > PICK_UP_THRESHOLD && !isPickedUp) {
    Serial.println("pickup");
    isPickedUp = true;
    delay(500);
  }

  if (z <= PUT_DOWN_THRESHOLD && isPickedUp) {
    Serial.println("putdown");
    isPickedUp = false;
    delay(500);
  }

  delay(10);
}
