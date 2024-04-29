import threading
import time
import queue
import speech_recognition as sr
import sys

q = queue.Queue()
recognized_text = ""

def get_audio_devices():
    audio_devices = sr.Microphone.list_microphone_names()
    return audio_devices

def write_audio_devices_to_file(devices):
    with open("audio_devices.txt", "w") as file:
        for idx, device in enumerate(devices):
            file.write(f"{idx}: {device}\n")

def write_text_to_file(text):
    with open("recognized_text.txt", "a", encoding="utf-8") as file:
        file.write(text + "\n")

def on_speech_recognized(recognizer, audio, language_code):
    try:
        text = recognizer.recognize_google(audio, language=language_code)
        print(text)
        recognized_text = text
        write_text_to_file(text)
    except sr.UnknownValueError:
        print("Could not understand audio")
    except sr.RequestError as e:
        print("Error fetching results; {0}".format(e))

def listen_background(device_index):
    recognizer = sr.Recognizer()
    with sr.Microphone(device_index=device_index) as source:
        while True:
            audio = recognizer.listen(source)
            q.put(audio)

def transcribe_audio(language_code):
    recognizer = sr.Recognizer()
    while True:
        if not q.empty():
            audio = q.get()
            on_speech_recognized(recognizer, audio, language_code)
        else:
            time.sleep(5)

def main(language_code):
    audio_devices = get_audio_devices()
    write_audio_devices_to_file(audio_devices)

    desired_device_index = int(sys.argv[1])

    listen_thread = threading.Thread(target=listen_background, args=(desired_device_index,))
    listen_thread.start()

    transcribe_thread = threading.Thread(target=transcribe_audio, args=(language_code,))
    transcribe_thread.start()

    listen_thread.join()
    transcribe_thread.join()

if __name__ == "__main__":
    language_code = sys.argv[2] if len(sys.argv) > 2 else "en-US"
    main(language_code)