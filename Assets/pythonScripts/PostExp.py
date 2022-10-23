import sys
import getopt


def test(arg):
    f = open(arg, "w")
    f.write("Now the file has more content!")
    f.close()


def main(argv):
    try:
        opts, args = getopt.getopt(argv, "f:",["file ="])
    except getopt.GetoptError:
        print("oh no");
        sys.exit(-1)

    for opt, arg in opts:
        if opt in ['f', '--file']:
            print(arg)
            test(arg)

if __name__ == "__main__":
    main(sys.argv[1:])