#----------------------------------------------------------
# M1. Actividad
# Este programa representa al modela de limpieza de habitacion
# 
# Date: 11-Nov-2022
# Authors:
#           Eduardo Joel Cortez Valente A01746664
#           Paulo Ogando Gulias A01751587
#----------------------------------------------------------

from mesa import Model
from mesa.time import SimultaneousActivation
from mesa.space import MultiGrid
from agents import *
import math

class AlmacenModelo(Model):

    # Inicializa el modelo. Agrega los agentes
    def __init__(self, tiempoMaximo, width, height, num_box):
        self.largo = width
        self.ancho = height
        self.max_time = tiempoMaximo - 2 
        self.num_robots = 5
        self.num_box = num_box
        self.boxes_recolected = 0
        self.grid = MultiGrid(width, height, False)
        self.schedule = SimultaneousActivation(self)
        self.running = True
        self.cont_time = 0
        self.num_shelfs = math.ceil(self.num_box / 5)

        border = [(x,y) for y in range(height) for x in range(width) if y in [0, height-1] or x in [0, width - 1]]
        doors = [(width-1,math.ceil((height-1)/2)),(width-1,math.ceil((height-1)/2)-1)]

        puerta = Puerta(0, self)
        self.schedule.add(puerta)
        self.grid.place_agent(puerta, doors[0])
        puerta = Puerta(1, self)
        self.schedule.add(puerta)
        self.grid.place_agent(puerta, doors[1])

        c = 2
        for pos in border:
            if pos != doors[0] and pos != doors[1]:
                obs = Pared(c, self)
                self.schedule.add(obs)
                self.grid.place_agent(obs, pos)
                c += 1

        numEstante = 0
        while(numEstante < self.num_shelfs):
            x = 1 + numEstante
            y = 1
            if x >= self.ancho - 1:
                x = 1
                y = 4 + (numEstante - self.ancho)
            estante = Estante(c, self)            
            self.schedule.add(estante)
            self.grid.place_agent(estante, (x, y))
            numEstante += 1
            c += 1

        numCajas = 0
        while(numCajas < self.num_box):
            x = self.random.randrange(self.grid.width)
            y = self.random.randrange(self.grid.height)
            cellmates = self.grid.get_cell_list_contents([(x,y)])
            if (not cellmates) and y != 1:
                basura = Caja(c, self)            
                self.schedule.add(basura)
                self.grid.place_agent(basura, (x, y))
                numCajas += 1
                c += 1

        numRobots = 0
        while(numRobots < self.num_robots):
            x = self.random.randrange(self.grid.width)
            y = self.random.randrange(self.grid.height)
            cellmates = self.grid.get_cell_list_contents([(x,y)])
            if (not cellmates):
                robot = Robot(c, self)            
                self.schedule.add(robot)
                self.grid.place_agent(robot, (x, y))
                numRobots += 1
                c += 1

    def check_steps(self):
        if self.cont_time == self.max_time:
            self.running = False
        elif self.boxes_recolected == self.num_box:
            self.running = False
        else:
            self.cont_time += 1

    # Representa un paso del modelo
    def step(self):
        self.check_steps()
        self.schedule.step()
