#----------------------------------------------------------
# M1. Actividad
# Este programa grafica visualmente el modelo y los agentes
# interacturando
# 
# Date: 11-Nov-2022
# Authors:
#           Eduardo Joel Cortez Valente A01746664
#           Paulo Ogando Gulias A01751587
#----------------------------------------------------------
import mesa
from model import AlmacenModelo
from robot_limpieza import *

# Funcion que dibuja a cada uno de los agentes
def agent_portrayal(agent):
    portrayal = {"Shape": "circle",
                 "Filled": "true",
                 "r": 0.5}
    
    if(isinstance(agent, Robot)):
        portrayal["Color"] = agent.color
        portrayal["Layer"] = 0
        portrayal["text_color"] = "white"
        portrayal["text"] = agent.movements
    
    if(isinstance(agent, Caja)):
            portrayal["Color"] = agent.color
            portrayal["Layer"] = 0
            portrayal["r"] = 0.2

    if (isinstance(agent, Estante)):
        portrayal["Color"] = "black"
        portrayal["Layer"] = 0
        portrayal["r"] = 0.5
        portrayal["text_color"] = "white"
        portrayal["text"] = agent.num_boxes

    return portrayal

Cajas = 10
ancho = 10
alto = 10
tiempoMaximo = 200

# Instantiate a canvas grid with its width and height in cells, and in pixels
grid = mesa.visualization.CanvasGrid(agent_portrayal, ancho, ancho, 500, 500)

server = mesa.visualization.ModularServer(AlmacenModelo,
                       [grid],
                       "AlmacenModelo",
                       {"tiempoMaximo":tiempoMaximo, "width":ancho, "height":alto, "num_box":Cajas})
server.port = 8521  # The default
server.launch()
