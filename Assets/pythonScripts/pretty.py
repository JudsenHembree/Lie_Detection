import glob
import os

def listFolders():
    files = glob.glob("../archives/*/", recursive=False)
    return files

def main():
    gloResults = []
    gloResultsOffset = []
    # get all participant folders
    participants = listFolders()
    # folder for each participant made when using the postExp.py script
    best = []
    bestOffset = []
    for p in participants:
        old = glob.glob(p + "/results/*.txt")
        for file in old:
            os.remove(file)
        res = glob.glob(p + "/results/*.csv")
        for r in res:
            head, _ = os.path.split(r)
            if r.find("ffset") != -1:
                f = open(r, "r")
                lines = f.readlines()
                data = lines[1].split(", ")
                data[3] = str(int(data[0])/20)
                if len(bestOffset) == 0:
                    bestOffset.append(data)
                elif bestOffset[0][3] < data[3]:
                    bestOffset[0] = data
                pretty = [
                            "-                      |",
                            "                       |",
                            "                       |",
                            "         -"
                        ]
                for i in range(len(data)):
                    middle = len(pretty[i])//2
                    middleman = list(pretty[i])
                    for j in range(len(data[i])):
                        middleman[middle + j] = data[i][j]
                    pretty[i] = ''.join(middleman)
                gloResultsOffset.append(data)

                o = open(str(head) + "/results_Offset_clean.txt", "w")
                outlines = [
                        "-------------------------------------------------------"+ \
                        "---------------------------",
                        "-Total Correct Guesses | Total False Positives | Total" + \
                        " False Negatives | Accuracy-",
                        ''.join(pretty),
                        "-------------------------------------------------------"+ \
                        "---------------------------"
                        ]

                for l in outlines:
                    o.write(l + '\n')
                o.close()
                f.close()
            else:
                f = open(r, "r")
                lines = f.readlines()
                data = lines[1].split(", ")
                data[3] = str(int(data[0])/20)
                if len(best) == 0:
                    best.append(data)
                elif best[0][3] < data[3]:
                    best[0] = data
                pretty = [
                            "-                      |",
                            "                       |",
                            "                       |",
                            "         -"
                        ]
                for i in range(len(data)):
                    middle = len(pretty[i])//2
                    middleman = list(pretty[i])
                    for j in range(len(data[i])):
                        middleman[middle + j] = data[i][j]
                    pretty[i] = ''.join(middleman)
                gloResults.append(data)

                o = open(str(head) + "/global_results.txt", "w")
                outlines = [
                        "-------------------------------------------------------"+ \
                        "---------------------------",
                        "-Total Correct Guesses | Total False Positives | Total" + \
                        " False Negatives | Accuracy-",
                        ''.join(pretty),
                        "-------------------------------------------------------"+ \
                        "---------------------------"
                        ]

                for l in outlines:
                    o.write(l + '\n')
                o.close()
                f.close()
    finalGloResults = [0.0,0.0,0.0,0.0]
    finalGloResultsOffset = [0.0,0.0,0.0,0.0]
    for result in gloResultsOffset:
        finalGloResultsOffset[0] += float(result[0])
        finalGloResultsOffset[1] += float(result[1])
        finalGloResultsOffset[2] += float(result[2])
        finalGloResultsOffset[3] += float(result[3])
    for result in gloResults:
        finalGloResults[0] += float(result[0])
        finalGloResults[1] += float(result[1])
        finalGloResults[2] += float(result[2])
        finalGloResults[3] += float(result[3])

    finalGloResultsOffset[0] = round(finalGloResultsOffset[0] / len(gloResultsOffset), 2)
    finalGloResultsOffset[1] = round(finalGloResultsOffset[1] / len(gloResultsOffset), 2)
    finalGloResultsOffset[2] = round(finalGloResultsOffset[2] / len(gloResultsOffset), 2)
    finalGloResultsOffset[3] = round(finalGloResultsOffset[3] / len(gloResultsOffset), 2)

    finalGloResults[0] = round(finalGloResults[0] / len(gloResults), 2)
    finalGloResults[1] = round(finalGloResults[1] / len(gloResults), 2)
    finalGloResults[2] = round(finalGloResults[2] / len(gloResults), 2)
    finalGloResults[3] = round(finalGloResults[3] / len(gloResults), 2)

    for p in participants:
        pretty = [
                    "-                      |",
                    "                       |",
                    "                       |",
                    "         -"
                ]
        for i in range(len(finalGloResults)):
            middle = len(pretty[i])//2
            middleman = list(pretty[i])
            for j in range(len(str(finalGloResults[i]))):
                middleman[middle + j] = str(finalGloResults[i])[j]
            pretty[i] = ''.join(middleman)
        gloResultsOffset.append(finalGloResults)

        o = open(p + "/results/global_clean.txt", "w")
        po = open(p + "/results/global_join.txt", "w")
        outlines = [
                "--------Global Averages Over all Participants----------"+ \
                "---------------------------",
                "-Total Correct Guesses | Total False Positives | Total" + \
                " False Negatives | Accuracy-",
                ''.join(pretty),
                "-------------------------------------------------------"+ \
                "---------------------------"
                ]

        for l in outlines:
            o.write(l + '\n')
        o.close()

        for l in outlines:
            po.write(l + '\n')

        pretty = [
                    "-                      |",
                    "                       |",
                    "                       |",
                    "         -"
                ]
        for i in range(len(best[0])):
            middle = len(pretty[i])//2
            middleman = list(pretty[i])
            for j in range(len(str(best[0][i]))):
                middleman[middle + j] = str(best[0][i])[j]
            pretty[i] = ''.join(middleman)

        o = open(p + "/results/best_participant.txt", "w")
        outlines = [
                "--------Best Participant Accuracy----------------------"+ \
                "---------------------------",
                "-Total Correct Guesses | Total False Positives | Total" + \
                " False Negatives | Accuracy-",
                ''.join(pretty),
                "-------------------------------------------------------"+ \
                "---------------------------"
                ]

        for l in outlines:
            o.write(l + '\n')
        o.close()

        for l in outlines:
            po.write(l + '\n')



        pretty = [
                    "-                      |",
                    "                       |",
                    "                       |",
                    "         -"
                ]
        for i in range(len(finalGloResultsOffset)):
            middle = len(pretty[i])//2
            middleman = list(pretty[i])
            for j in range(len(str(finalGloResultsOffset[i]))):
                middleman[middle + j] = str(finalGloResultsOffset[i])[j]
            pretty[i] = ''.join(middleman)
        gloResultsOffset.append(finalGloResultsOffset)

        o = open(p + "/results/global_Offset_clean.txt", "w")
        outlines = [
                "--------Global Averages Over all Participants w/Offset-"+ \
                "---------------------------",
                "-Total Correct Guesses | Total False Positives | Total" + \
                " False Negatives | Accuracy-",
                ''.join(pretty),
                "-------------------------------------------------------"+ \
                "---------------------------"
                ]

        for l in outlines:
            o.write(l + '\n')
        o.close()

        for l in outlines:
            po.write(l + '\n')


        pretty = [
                    "-                      |",
                    "                       |",
                    "                       |",
                    "         -"
                ]
        for i in range(len(bestOffset[0])):
            middle = len(pretty[i])//2
            middleman = list(pretty[i])
            for j in range(len(str(bestOffset[0][i]))):
                middleman[middle + j] = str(bestOffset[0][i])[j]
            pretty[i] = ''.join(middleman)

        o = open(p + "/results/best_offset_participant.txt", "w")
        outlines = [
                "--------Best Participant Accuracy with offset----------"+ \
                "---------------------------",
                "-Total Correct Guesses | Total False Positives | Total" + \
                " False Negatives | Accuracy-",
                ''.join(pretty),
                "-------------------------------------------------------"+ \
                "---------------------------"
                ]

        for l in outlines:
            o.write(l + '\n')
        o.close()

        for l in outlines:
            po.write(l + '\n')



        po.close()


if __name__ == "__main__":
    main()
