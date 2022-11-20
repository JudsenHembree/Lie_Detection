import glob
import os
import shutil


def test(arg):
    f = open(arg, "w")
    f.write("Now the file has more content!")
    f.close()

def listFiles():
    files = glob.glob("../Data/*.csv", recursive=False)
    return files

def move(files):
    if not os.path.exists("../archives"):
        os.mkdir("../archives")

    folders = glob.glob("../archives/*/")
    destination = "../archives/participant_" + str(len(folders))
    os.mkdir(destination)
    os.mkdir(destination + "/rawData")
    for file in files:
        _, tail = os.path.split(file)
        shutil.copyfile(file, destination + "/rawData/" + tail)

def main():
    files = listFiles()
    print(files)
    move(files)

if __name__ == "__main__":
    main()
