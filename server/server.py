# TC2008B. Sistemas Multiagentes y Gráficas Computacionales
# Python flask server to interact with Unity. Based on the code provided by Sergio Ruiz.
# Octavio Navarro. October 2021

from flask import Flask, request, jsonify
from model import *

# Size of the board:
# cajas = 40
# ancho = 10
# alto = 10
# tiempoMaximo = 200

# randomModel = None
# currentStep = 0

app = Flask("Traffic example")

# @app.route('/', methods=['POST', 'GET'])

@app.route('/init', methods=['POST', 'GET'])
def initModel():
    global currentStep, randomModel, number_agents, width, height

    if request.method == 'POST':
        cajas = int(request.form.get('NAgents'))
        ancho = int(request.form.get('width'))
        alto = int(request.form.get('height'))
        tiempoMaximo = int(request.form.get('maxtime'))
        currentStep = 0

        print(request.form)
        print(cajas, ancho, alto)
        randomModel = AlmacenModelo(tiempoMaximo, alto, ancho, cajas)

        return jsonify({"message":"Parameters recieved, model initiated."})

@app.route('/getRobots', methods=['GET'])
def getRobots():
    global randomModel

    if request.method == 'GET':
        robotPositions = [{"id": str(agent.unique_id), "x": x, "y":0, "z":z, "has_box":agent.has_box} for (a, x, z) in randomModel.grid.coord_iter() for agent in a if isinstance(agent, Robot)]

        return jsonify({'positions':robotPositions})

@app.route('/getEstantes', methods=['GET'])
def getEstantes():
    global randomModel

    if request.method == 'GET':
        estantePositions = [{"id": str(agent.unique_id), "x": x, "y":0, "z":z, "status":agent.current_boxes} for (a, x, z) in randomModel.grid.coord_iter() for agent in a if isinstance(agent, Estante)]

        return jsonify({'positions':estantePositions})

@app.route('/getCajas', methods=['GET'])
def getCajas():
    global randomModel

    if request.method == 'GET':
        cajaPositions = [{"id": str(agent.unique_id), "x": x, "y":0, "z":z, "status":agent.status} for (a, x, z) in randomModel.grid.coord_iter() for agent in a if isinstance(agent, Caja)]

        return jsonify({'positions':cajaPositions})

@app.route('/getPuertas', methods=['GET'])
def getPuertas():
    global randomModel

    if request.method == 'GET':
        puertasPosition = [{"id": str(agent.unique_id), "x": x, "y":0, "z":z} for (a, x, z) in randomModel.grid.coord_iter() for agent in a if isinstance(agent, Puerta) ]
        return jsonify({'positions':puertasPosition})

@app.route('/getParedes', methods=['GET'])
def getParedes():
    global randomModel

    if request.method == 'GET':
        paredesPosition = [{"id": str(agent.unique_id), "x": x, "y":0, "z":z} for (a, x, z) in randomModel.grid.coord_iter() for agent in a if isinstance(agent, Pared) ]
        return jsonify({'positions':paredesPosition})

@app.route('/update', methods=['GET'])
def updateModel():
    global currentStep, randomModel
    if request.method == 'GET':
        randomModel.step()
        currentStep += 1
        return jsonify({'message':f'Model updated to step {currentStep}.', 'currentStep':currentStep})

if __name__=='__main__':
    app.run(host="localhost", port=8585, debug=True)
