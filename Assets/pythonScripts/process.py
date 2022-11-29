from matplotlib import pyplot
from scipy.signal import savgol_filter
import glob
import shutil
import os
import pandas
import numpy

def proxy(x):
    print(x)
    return savgol_filter(x, 400, 1)


def listFolders():
    files = glob.glob("../archives/*/", recursive=False)
    return files

def processPartialCSV(part, figs, results):
        # name the fig
        _, tail = os.path.split(part)
        pyplot.title(tail)
        # read the csv and create custom labels
        csv = pandas.read_csv(part, \
                names=["time", "status", "questionStatus", "left", \
                "right"], skiprows=1)

        # drop the last two rows
        csv.drop(csv.tail(2).index, inplace=True)
        csv['left'] = csv['left'].replace(-1, numpy.nan)
        csv['left'] = csv['left'].interpolate()
        

        csv['right'] = csv['right'].replace(-1, numpy.nan)
        csv['right'] = csv['right'].interpolate()

        #baseline filter
        base = csv[(csv.left != -1) & \
                (csv.questionStatus.str.contains('baseline', case=False))]
        #response filter
        response = csv[(csv.left != -1) & \
                (csv.questionStatus.str.contains('response', case=False))]
        #response filter sus
        responseSus = csv[(csv.left != -1) & \
                (csv.questionStatus.str.contains('response', case=False)) \
                & (csv.left >= (numpy.nanmean(base.left) + \
                (numpy.std(base.left) * 2)))]
        #question filter
        question = csv[(csv.left != -1) & \
                (csv.questionStatus.str.contains('question', case=False))]
        #question filter sus
        questionSus = csv[(csv.left != -1) & \
                (csv.questionStatus.str.contains('question', case=False)) \
                & (csv.left >= (numpy.nanmean(base.left) + \
                (numpy.std(base.left) * 2)))]

        susCoef = ((len(responseSus.index) + len(questionSus.index))/ \
                (len(response.index) + len(question.index)))

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
        val = val.strip()
        val = val.capitalize()
        if susCoef > .25:
            sus = "Lying"
        else:
            sus = "Truthful"
        f.suptitle(val + "\n Prediction is: " + sus)
        #finalize
        f.canvas.draw()
        f.savefig(figs + "/raw/" + tail + ".png", dpi=f.dpi)
        #close fig don't hog memeory
        pyplot.close(f)


        left = csv[['left']].copy()
        left_smoothed = pandas.DataFrame(savgol_filter(left, 41, 1, axis=0),
                                columns=left.columns,
                                index=left.index)

        right = csv[['right']].copy()
        right_smoothed = pandas.DataFrame(savgol_filter(right, 41, 1, axis=0),
                                columns=right.columns,
                                index=right.index)

        csv['left'] = left_smoothed
        csv['right'] = right_smoothed

        #baseline filter
        base = csv[(csv.left != -1) & \
                (csv.questionStatus.str.contains('baseline', case=False))]
        #response filter
        response = csv[(csv.left != -1) & \
                (csv.questionStatus.str.contains('response', case=False))]
        #response filter sus
        responseSus = csv[(csv.left != -1) & \
                (csv.questionStatus.str.contains('response', case=False)) \
                & (csv.left >= (numpy.nanmean(base.left) + \
                (numpy.std(base.left) * 2)))]
        #question filter
        question = csv[(csv.left != -1) & \
                (csv.questionStatus.str.contains('question', case=False))]
        #question filter sus
        questionSus = csv[(csv.left != -1) & \
                (csv.questionStatus.str.contains('question', case=False)) \
                & (csv.left >= (numpy.nanmean(base.left) + \
                (numpy.std(base.left) * 2)))]

        susCoef = ((len(responseSus.index) + len(questionSus.index))/ \
                (len(response.index) + len(question.index)))

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
        val = val.strip()
        val = val.capitalize()
        if susCoef > .4:
            sus = "Lying"
        else:
            sus = "Truthful"
        f.suptitle(val + "\n Prediction is: " + sus)
        #finalize
        f.canvas.draw()
        f.savefig(figs + "/smoothed/" + tail + ".png", dpi=f.dpi)
        #close fig don't hog memeory
        pyplot.close(f)



        general = csv[(csv.left != -1) & \
                ((csv.questionStatus.str.contains('response', case=False)) \
                | (csv.questionStatus.str.contains('question', case=False)))]

        generalLeft = general["left"]

        diff = generalLeft.diff()

        #create fig
        d, quest = pyplot.subplots(figsize=(6,3))
        #plot response segment
        quest.plot(general.time, diff)
        # title indicates intention truth/lie
        val = base["status"].values[0]
        val = val.strip()
        val = val.capitalize()
        d.suptitle(str(val) + "\n raw differ")
        #finalize
        ax = pyplot.gca()
        ax.set_ylim([-0.01, 0.01])

        d.canvas.draw()
        d.savefig(figs + "/rawDiffer/" + tail + ".png", dpi=d.dpi)
        #close fig don't hog memeory
        pyplot.close(d)


        right = csv[['right']].copy()
        right_smoothed = pandas.DataFrame(savgol_filter(right, 41, 1, axis=0),
                                columns=right.columns,
                                index=right.index)

        general = csv[(csv.left != -1) & \
                ((csv.questionStatus.str.contains('response', case=False)) \
                | (csv.questionStatus.str.contains('question', case=False)))]

        generalLeft = general[["left"]].copy()

        diff = generalLeft.diff()
        diff = pandas.DataFrame(savgol_filter(diff, 41, 1, axis=0),
                                columns=diff.columns,
                                index=diff.index)

        #create fig
        d, quest = pyplot.subplots(figsize=(6,3))
        #plot response segment
        quest.plot(general.time, diff)
        # title indicates intention truth/lie
        val = base["status"].values[0]
        val = val.strip()
        val = val.capitalize()
        d.suptitle(str(val) + "\nsmoothed diff")
        #finalize
        ax = pyplot.gca()
        ax.set_ylim([-0.01, 0.01])

        d.canvas.draw()
        d.savefig(figs + "/smoothDiffer/" + tail + ".png", dpi=d.dpi)
        #close fig don't hog memeory
        pyplot.close(d)


        if sus == val:
            print("right")
            results[0] += 1
            if sus == "Lying":
                results[3] += 1
        if sus == "Lying" and val == "Truthful":
            print("false positive")
            results[1] += 1
        if sus == "Truthful" and val == "Lying":
            print("false negative")
            results[2] += 1

def logResults(results, f):
    f.write("correct, false positive, false negative, predicted lie successfully\n")
    f.write(str(results[0]) + ", " + str(results[1]) + ", " + str(results[2]) + ", " + str(results[3]))

def main():
    # get all participant folders
    participants = listFolders()
    pyplot.rcParams["figure.autolayout"] = True
    # folder for each participant made when using the postExp.py script
    for p in participants:
        priorFiles = glob.glob(p + '*')
        for prior in priorFiles:
            if os.path.isdir(prior):
                _, tail = os.path.split(prior)
                if(tail != "rawData"):
                    shutil.rmtree(prior)
            if os.path.isfile(prior):
                os.remove(prior)

        results = [0, 0, 0, 0]
        partials = p + "/rawData/*partial.csv"
        fulls = p + "/rawData/*full.csv"
        figs = p + "figs"
        resultsFolder = p + "/results"
        if not os.path.exists(figs):
            os.mkdir(figs)
        if not os.path.exists(figs + "/rawDiffer"):
            os.mkdir(figs + "/rawDiffer")
        if not os.path.exists(figs + "/smoothDiffer"):
            os.mkdir(figs + "/smoothDiffer")
        if not os.path.exists(figs + "/smoothed"):
            os.mkdir(figs + "/smoothed")
        if not os.path.exists(figs + "/raw"):
            os.mkdir(figs + "/raw")
        if not os.path.exists(resultsFolder):
            os.mkdir(resultsFolder)
        # list of each partialCSV
        partialCSVs = glob.glob(partials, recursive=False)
        # list of each fullCSV
        fullCSVs = glob.glob(fulls, recursive=False)
        # for each partialcsv
        for part in partialCSVs:
            processPartialCSV(part, figs, results)

        f = open(resultsFolder + "/results.csv", "w")
        logResults(results, f)

if __name__ == "__main__":
    main()
