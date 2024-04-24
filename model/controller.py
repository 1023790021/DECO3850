import cv2
import mediapipe as mp
import time
import socket
import pickle
import numpy as np

# Setup socket for Unity connection
UNITY_IP = '127.0.0.1'
UNITY_PORT = 3850
server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server_socket.bind((UNITY_IP, UNITY_PORT))
server_socket.listen(5)
print(f"Listening for connections on {UNITY_IP}:{UNITY_PORT}...")
client_socket, addr = server_socket.accept()
print(f"Connected to {addr}")

# Setup camera
capture = cv2.VideoCapture(0)
if not capture.isOpened():
    print("Camera opened failed")
else:
    print("Camera opened successfully")

# Define constants
DISPLACEMENT_THRESHOLD = 1

# Initialize MediaPipe solutions
mp_hands = mp.solutions.hands
hands = mp_hands.Hands(min_detection_confidence=0.7, min_tracking_confidence=0.7)
mp_pose = mp.solutions.pose
pose = mp_pose.Pose(min_detection_confidence=0.7, min_tracking_confidence=0.7)
mp_drawing = mp.solutions.drawing_utils

# Load SVM models (ensure the paths are correct)
with open('model.pkl', 'rb') as file:
    action_clf = pickle.load(file)

# Initialize variables to store previous wrist and index finger tip positions
prev_wrist_x = 0
prev_index_finger_tip_x = 0

try:
    while capture.isOpened():
        success, image = capture.read()
        if not success:
            print("Ignoring empty camera frame.")
            continue

        # Convert the BGR image to RGB
        image = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)

        # MediaPipe hands detection
        hands_results = hands.process(image)
        # If hand landmarks are detected
        if hands_results.multi_hand_landmarks:
            for hand_landmarks in hands_results.multi_hand_landmarks:
                # Get wrist and index finger tip positions
                wrist_x = hand_landmarks.landmark[mp_hands.HandLandmark.WRIST].x
                index_finger_tip_x = hand_landmarks.landmark[mp_hands.HandLandmark.INDEX_FINGER_TIP].x

                # Calculate displacement
                displacement_wrist = wrist_x - prev_wrist_x
                displacement_finger = index_finger_tip_x - prev_index_finger_tip_x

                # Calculate total displacement
                total_displacement = displacement_wrist + displacement_finger
                # If displacement is above threshold
                if abs(total_displacement) > DISPLACEMENT_THRESHOLD:
                    # Determine direction of movement
                    direction = "left" if total_displacement < 0 else "right"
                    print(f"Hand swings to the {direction}")

                    # Send the direction information to Unity
                    try:
                        client_socket.sendall(direction.encode())
                    except Exception as e:
                        print(f"Failed to send data to Unity: {e}")

                # Update previous positions
                prev_wrist_x = wrist_x
                prev_index_finger_tip_x = index_finger_tip_x

                # Draw hand landmarks on image
                mp_drawing.draw_landmarks(image, hand_landmarks, mp_hands.HAND_CONNECTIONS)

        # MediaPipe pose detection
        pose_results = pose.process(image)
        if pose_results.pose_landmarks:
            mp_drawing.draw_landmarks(image, pose_results.pose_landmarks, mp_pose.POSE_CONNECTIONS)

            # Extract features for classification
            features = [[landmark.x, landmark.y, landmark.z] for landmark in pose_results.pose_landmarks.landmark]
            features_flattened = np.array([item for sublist in features for item in sublist]).reshape(1, -1)

            # Action detection using the combined model
            action = action_clf.predict(features_flattened)[0]
            cv2.putText(image, f"{action} Detected", (10, 30), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2)
            client_socket.sendall(action.encode())

        # Show the image
        cv2.imshow('MediaPipe Hands & Pose', image)
        if cv2.waitKey(5) & 0xFF == 27:
            break

except Exception as e:
    print(f"An error occurred: {e}")
finally:
    capture.release()
    cv2.destroyAllWindows()
    client_socket.close()
    server_socket.close()
