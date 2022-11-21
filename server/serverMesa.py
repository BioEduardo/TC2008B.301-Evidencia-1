#----------------------------------------------------------
# Evidencia 1. Actividad Integradora
# Este programa grafica visualmente el modelo y los agentes
# interacturando en Mesa
# 
# Date: 21-Nov-2022
# Authors:
#           Eduardo Joel Cortez Valente A01746664
#           Paulo Ogando Gulias A01751587
#           David Damián Galán A01752785
#           José Ángel García Gómez A01745865
#----------------------------------------------------------
import mesa
from model import AlmacenModelo
from agents import *

# Funcion que dibuja a cada uno de los agentes
def agent_portrayal(agent):
    portrayal = {"Shape": "circle",
                 "Filled": "true",
                 "r": 0.5}
    
    if(isinstance(agent, Robot)):
        portrayal["Color"] = agent.color
        portrayal["Layer"] = 0
        portrayal["text_color"] = "white"
        portrayal["text"] = agent.has_box
    
    if(isinstance(agent, Caja)):
            portrayal["Color"] = agent.color
            portrayal["Layer"] = 0
            portrayal["r"] = 0.2

    if (isinstance(agent, Estante)):
        portrayal["Color"] = "black"
        portrayal["Layer"] = 0
        portrayal["r"] = 0.5
        portrayal["text_color"] = "white"
        portrayal["text"] = agent.current_boxes

    if (isinstance(agent, Pared)):
        portrayal["Color"] = "grey"
        portrayal["Layer"] = 0
        portrayal["r"] = 0.5

    if (isinstance(agent, Puerta)):
        portrayal["Color"] = "purple"
        portrayal["Layer"] = 0
        portrayal["r"] = 0.5

    return portrayal

Cajas = 20
ancho = 10
alto = 10
tiempoMaximo = 500

# Instantiate a canvas grid with its width and height in cells, and in pixels
grid = mesa.visualization.CanvasGrid(agent_portrayal, ancho, ancho, 500, 500)

server = mesa.visualization.ModularServer(AlmacenModelo,
                       [grid],
                       "AlmacenModelo",
                       {"tiempoMaximo":tiempoMaximo, "width":ancho, "height":alto, "num_box":Cajas})
server.port = 8521  # The default
server.launch()
