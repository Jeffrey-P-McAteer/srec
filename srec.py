
import subprocess
import time
import os 
import sys
import socket
import traceback

vlc_proc = subprocess.Popen([
    r'C:\Program Files\VideoLAN\VLC\vlc.exe',
    'screen://',
    '--one-instance',
    '-I', 'dummy', # '--dummy-quiet',
    '--extraintf', 'rc', '--rc-host', '127.0.0.1:8080', # '--rc-quiet',
    '--no-screen-follow-mouse',
    '--screen-left=0', '--screen-top=0', '--screen-width=1920', '--screen-height=1080',
    '--no-video', ':screen-fps=10', ':screen-caching=100',
    '--sout', "#transcode{vcodec=h264,vb=0,fps=10,scale=1,acodec=none}:duplicate{dst=std{access=file,mux=mp4,dst='video.mp4'}}"
])

time.sleep(0.5)

input('[ Press any key to stop ]')


try:
    s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    s.connect(('127.0.0.1', 8080))
    s.sendall(b'quit\n')
    s.shutdown(socket.SHUT_WR)
except:
    traceback.print_exc()

time.sleep(1.75)

try:
    vlc_proc.kill()
except:
    traceback.print_exc()
    

