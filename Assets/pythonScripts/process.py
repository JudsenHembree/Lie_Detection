from matplotlib import pyplot
import glob
import os
import pandas
import numpy

def listFolders():
    files = glob.glob("../archives/*/", recursive=False)
    return files

def processPartialCSV(part, figs):
        # name the fig
        _, tail = os.path.split(part)
        pyplot.title(tail)
        # read the csv and create custom labels
        csv = pandas.read_csv(part, \
                names=["time", "status", "questionStatus", "left", \
                "right"], skiprows=1)
        # drop the last two rows
        csv.drop(csv.tail(2).index, inplace=True)

        #baseline filter
        base = csv[(csv.left != -1) & \
                (csv.questionStatus.str.contains('baseline', case=False))]
        #response filter
        response = csv[(csv.left != -1) & \
                (csv.questionStatus.str.contains('response', case=False))]
        #question filter
        question = csv[(csv.left != -1) & \
                (csv.questionStatus.str.contains('question', case=False))]

        #create fig
        f, quest = pyplot.subplots(figsize=(6,3))
        #mean line
        quest.axhline(y=numpy.nanmean(base.left))
        #stddev +1 line
        quest.axhline(y=(numpy.nanmean(base.left) +\
                numpy.std(base.left)), color="green")
        #stddev +2 line
        quest.axhline(y=(numpy.nanmean(base.left) +\
                (numpy.std(base.left) * 2)), color="red")
        #stddev -1 line
        quest.axhline(y=(numpy.nanmean(base.left) -\
                numpy.std(base.left)), color="green")
        #stddev -2 line
        quest.axhline(y=(numpy.nanmean(base.left) -\
                (numpy.std(base.left) * 2)), color="red")

        #plot response segment
        quest.plot(response.time, response.left)
        #plot question segment
        quest.plot(question.time, question.left)
        # title indicates intention truth/lie
        val = base["status"].values[0]
        f.suptitle(val)
        #finalize
        f.canvas.draw()
        f.savefig(figs + "/" + tail + ".png", dpi=f.dpi)
        #close fig don't hog memeory
        pyplot.close(f)


def main():
    # get all participant folders
    participants = listFolders()
    pyplot.rcParams["figure.autolayout"] = True

    # folder for each participant made when using the postExp.py script
    for p in participants:
        partials = p + "/*partial.csv"
        fulls = p + "/*full.csv"
        figs = p + "/figs"
        if not os.path.exists(figs):
            os.mkdir(figs)
        # list of each partialCSV
        partialCSVs = glob.glob(partials, recursive=False)
        # list of each fullCSV
        fullCSVs = glob.glob(fulls, recursive=False)
        # for each partialcsv
        for part in partialCSVs:
            processPartialCSV(part, figs)

if __name__ == "__main__":
    main()
