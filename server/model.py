#----------------------------------------------------------
# M1. Actividad
# Este programa representa al modela de limpieza de habitacion
# 
# Date: 11-Nov-2022
# Authors:
#           Eduardo Joel Cortez Valente A01746664
#           Paulo Ogando Gulias A01751587
#----------------------------------------------------------

from mesa import Model, DataCollector
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
        self.grid = MultiGrid(width, height, True)
        self.schedule = SimultaneousActivation(self)
        self.running = True
        self.cont_time = 0
        self.num_shelfs = math.ceil(self.num_box / 5)

        c = 0
        for _ in range(self.num_shelfs):
            x = c
            y = 0
            if x >= self.ancho:
                x = 0
                y = 1 + (c - self.ancho)
            estante = Estante(c, self)            
            self.schedule.add(estante)
            self.grid.place_agent(estante, (x, y))
            c += 1

        while((c - self.num_robots) < self.num_box):
            x = self.random.randrange(self.grid.width)
            y = self.random.randrange(self.grid.height)
            cellmates = self.grid.get_cell_list_contents([(x,y)])
            if (not cellmates):
                basura = Caja(c, self)            
                self.schedule.add(basura)
                self.grid.place_agent(basura, (x, y))
                c += 1

        while((c - self.num_shelfs - self.num_box) < self.num_robots):
            x = self.random.randrange(self.grid.width)
            y = self.random.randrange(self.grid.height)
            cellmates = self.grid.get_cell_list_contents([(x,y)])
            if (not cellmates):
                robot = Robot(c, self)            
                self.schedule.add(robot)
                self.grid.place_agent(robot, (x, y))
                c += 1

    # Checa si se ha alcanzado el limite de tiempo establecido    
    
    # Checa si toda la basura ha sido limpiada

    # Representa un paso del modelo
    def step(self):
        self.schedule.step()
