#include <Adafruit_CircuitPlayground.h>

const float PICK_UP_THRESHOLD = 1.0;
const float PUT_DOWN_THRESHOLD = 0.1;
bool isPickedUp = false;
const float LEFT_MOVE_THRESHOLD = -10.0;
const float RIGHT_MOVE_THRESHOLD = 10.0;

void setup() {
  Serial.begin(9600);
  CircuitPlayground.begin();
}

void loop() {
  // Read acceleration data for x, y, and z axes
  float x = CircuitPlayground.motionX();
  float y = CircuitPlayground.motionY();
  float z = CircuitPlayground.motionZ();

  // if (z <= PICK_UP_THRESHOLD && !isPickedUp) {
  //   Serial.println("pickup");
  //   isPickedUp = true;
  //   delay(500);
  // }

  // if (z >= PUT_DOWN_THRESHOLD && isPickedUp) {
  //   Serial.println("putdown");
  //   isPickedUp = false;
  //   delay(500);
  // }

  // if (x <= LEFT_MOVE_THRESHOLD) {
  //   Serial.println("Moved Left");
  //   delay(500);
  // }

  // if (x >= RIGHT_MOVE_THRESHOLD) {
  //   Serial.println("Moved Right");
  //   delay(500);
  // }

  // delay(100);

  if (Serial.available() > 0) {
    String data = Serial.readStringUntil('\n');
    Serial.println("Received: " + data);

    if (data == "playSound") {
      Serial.println("Playing sound");
      CircuitPlayground.playTone(300, 500);
    } else if (data == "stopSound") {
      CircuitPlayground.playTone(300, 0);
    }

    if (data == "lightOn") {
      Serial.println("Lighting up");
      for (int i = 0; i < 10; i++) {
        CircuitPlayground.setPixelColor(i, CircuitPlayground.colorWheel(i * 25));
      }
      delay(500);
    } else if (data == "lightOff") {
        CircuitPlayground.clearPixels();
    }
  }
}
