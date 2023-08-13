from fastapi import FastAPI
from fastapi.responses import FileResponse
import random
import pandas as pd
import numpy as np
import json
import base64

app = FastAPI()
df = pd.read_excel("data.xlsx")  # basic info for all markers
df_detail = pd.read_excel("details.xlsx")  # details for all exhibits(not for something like entrance in this demo)


# Main page, the basic url to test whether the server itself is working properly.
@app.get("/")
def read_root():
    return {"Hello": "World"}


# Location infos, return all the infos needed to fill in the UI menu
@app.get("/loinfo")
def read_root():
    landmarks_info = construct_json()
    return landmarks_info


# Construct a correct json string which will be transfered to Unity Client
def construct_json():
    # classname represents the name of the classgroup created in Unity, which handles data transfer
    classname = "tritexts"

    # labels is a list of all the member variable in a single class unit
    labels = ["name", "info", "position", "imagestr"]

    # construct the unit one by one based on the data.xlsx. Each unit is a dictionary.
    info = []
    for r in range(len(df.index)):
        row = df.values[r, :]
        imagepath = "./pics/" + row[1] + ".png"
        image = open(imagepath, "rb")
        image_base64 = base64.b64encode(image.read())
        unit = {labels[0]:row[1],labels[1]:row[2],labels[2]:{"x":row[3],"y":row[4],"z":row[5]},labels[3]:image_base64}
        info.append(unit)
    # print(info)

    # pack the unit list into a single-label dictionary, and Unity will receive and process the json string
    # as a single classgroup variable. Refer to relevant Csharp script for better understanding.
    return {classname:info}


# Previous pre-stored data, replaced by excel files and folders for more flexibility and extensibility.
# infos={"tritexts":[{"name":"entrance","info":"Get in or out museum","position":{"x":7.408886,"y":0.056,"z":55.94}},
# {"name":"the_cool_sphere","info":"This is a sphere","position":{"x":56.13,"y":0.038,"z":82.61}},
# {"name":"tunnels","info":"These are tunnels","position":{"x":69.59,"y":0.28,"z":62.44}},
# {"name":"orange_painting","info":"This is an orange paintinig","position":{"x":31.46,"y":0.28,"z":32.25}},
# {"name":"red_painting","info":"This is a red paintinig","position":{"x":65.29,"y":7.29,"z":75.65}},
# {"name":"green_painting","info":"This is a green paintinig","position":{"x":65.29,"y":7.29,"z":75.65}},
# {"name":"statue","info":"This is a statue","position":{"x":56.10,"y":7.289,"z":81.97}},
# {"name":"stage","info":"This is a stage","position":{"x":21.54,"y":7.289,"z":35.54}},
# {"name":"toilet1","info":"This is toilet1","position":{"x":20.804,"y":0.056,"z":46.341}},
# {"name":"toilet2","info":"This is toilet2","position":{"x":88.04,"y":7.289,"z":76.5}},
# {"name":"shop","info":"This is shop","position":{"x":14.69,"y":7.289,"z":54.4}}]
# }


# location_format = {'name':'','info':'','position':''}


# Generate random people, provide position for creating human models in the scene
@app.get("/random_people")
def get_random_people():
    random_people_related = generate_random_people()
    return random_people_related
    

# 定义随机生成的人数范围 (set the range of the number of people generated in a single event)
min_people = 99
max_people = 101


# 随机生成人数并分配给人物 (generate random people and their positions)
def generate_random_people():
    num_people = random.randint(min_people, max_people)
    people_related = []

    for _ in range(num_people):
        # 随机分配人物在Unity场景中的位置 (random people will at least not fall out of the plain where museum stands)
        x = random.uniform(-20, 121)
        z = random.uniform(-17.8, 126)
        rand_num = random.random()
    
        # 如果生成的随机数小于 0.5，则将 y 的值设置为 1.5，否则设置为 9 (the two values represent different floors)
        if rand_num < 0.5:
            y = 0.09
        else:
            y = 7.29

        # 添加人物数据到列表中 (add position data to the list)
        person_related = {"transpose":{"x":x,"y":y,"z":z}}

        people_related.append(person_related)

    jsonstring = {"positions":people_related}  # construct json string

    return jsonstring


# Provide different kinds of details for exhibits. Text and audio are offered in this demo.
@app.get("/details")
def read_item(name: str, dtype: int):
    strindexs = df_detail.values[:, 0]
    if name in strindexs:
        index = np.where(strindexs == name)
    else:
        return "Error: Item not found."
    if len(index) != 1:
        print("Warning: Duplicate name index, only the first piece will return.")
    if dtype == 0:  # text
        ix = int(index[0])
        detailstr = df_detail.values[ix, 1]
        return detailstr
    elif dtype == 1:  # audio
        audiopath = "./mp3/" + name + ".mp3"
        return FileResponse(audiopath, media_type="audio/mpeg")
    else:  # the following codes are sample for video implementation
        # videopath = "./videos/" + name + ".mp4"
        # return FileResponse(videopath, media_type="video/mp4")
        return "Error: Wrong data type!"


# Sensor initialization, bind each trigger with correct exhibit name.
@app.get("/sensor_init")
def read_item(sname: str):
    sensor_names = df_detail.values[:, 2]
    if sname in sensor_names:
        index = np.where(sensor_names == sname)
        if len(index) != 1:
            print("Warning: Duplicate sname index, only the first piece will return.")
        ix = int(index[0])
        return df_detail.values[ix, 0]
    else:
        return "Error: Sensor name not found."


if __name__ == "__main__":
    import uvicorn

    uvicorn.run(
        "APP:app",
        host="localhost",
        reload=True,
        port=8000)
