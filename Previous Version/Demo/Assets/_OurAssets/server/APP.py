from fastapi import FastAPI
import random
import json

app = FastAPI()

@app.get("/")
def read_root():
    return {"Hello": "World"}
    
@app.get("/loinfo")
def read_root():
    return landmarks_info

 
landmarks_info={"tritexts":[{"name":"entrance","info":"Get in or out museum","position":{"x":7.408886,"y":0.056,"z":55.94}},
{"name":"the_cool_sphere","info":"This is a sphere","position":{"x":56.13,"y":0.038,"z":82.61}},
{"name":"tunnels","info":"These are tunnels","position":{"x":69.59,"y":0.28,"z":62.44}},
{"name":"orange_painting","info":"This is an orange paintinig","position":{"x":31.46,"y":0.28,"z":32.25}},
{"name":"red_painting","info":"This is a red paintinig","position":{"x":65.29,"y":7.29,"z":75.65}},
{"name":"green_painting","info":"This is a green paintinig","position":{"x":65.29,"y":7.29,"z":75.65}},
{"name":"statue","info":"This is a statue","position":{"x":56.10,"y":7.289,"z":81.97}},
{"name":"stage","info":"This is a stage","position":{"x":21.54,"y":7.289,"z":35.54}},
{"name":"toilet1","info":"This is toilet1","position":{"x":20.804,"y":0.056,"z":46.341}},
{"name":"toilet2","info":"This is toilet2","position":{"x":88.04,"y":7.289,"z":76.5}},
{"name":"shop","info":"This is shop","position":{"x":14.69,"y":7.289,"z":54.4}}]
}



location_format = {'name':'','info':'','position':''}


    

@app.get("/random_people")
def get_random_people():
    random_people_related = generate_random_people()
    return random_people_related
    

    
# 定义随机生成的人数范围
min_people = 99
max_people = 101

# 定义Unity博物馆模型中的人物名称
person_prefab_name = "PersonPrefab"

# 随机生成人数并分配给人物
def generate_random_people():
    num_people = random.randint(min_people, max_people)  # 生成随机人数
    people_related = []

    for _ in range(num_people):
        # 随机分配人物在Unity场景中的位置
        x = random.uniform(-20, 121)
        z = random.uniform(-17.8, 126)
        rand_num = random.random()
    
        # 如果生成的随机数小于 0.5，则将 y 的值设置为 1.5，否则设置为 9
        if rand_num < 0.5:
            y = 0.09
        else:
            y = 7.29

        # 添加人物数据到列表中
        person_related = {"transpose":{"x":x,"y":y,"z":z}}  # 假设高度为0，小方块是人

        people_related.append(person_related)

    jsonstring = {"positions":people_related}

    return jsonstring


if __name__ == "__main__":
     import uvicorn

     uvicorn.run(
         "APP:app",
         host="localhost",
         reload=True,
         port=8000)

