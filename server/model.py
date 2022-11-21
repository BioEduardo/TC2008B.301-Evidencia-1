#----------------------------------------------------------
# Evidencia 1. Actividad Integradora
# Este programa representa al modelo de ordenar cajas en un
# almacen
# 
# Date: 21-Nov-2022
# Authors:
#           Eduardo Joel Cortez Valente A01746664
#           Paulo Ogando Gulias A01751587
#           David Damián Galán A01752785
#           José Ángel García Gómez A01745865
#----------------------------------------------------------

from mesa import Model
from mesa.time import SimultaneousActivation
from mesa.space import MultiGrid
from agents import *
import math

class AlmacenModelo(Model):
    """ 
    Crea el modelo con paredes, puertas, cajas, estantes y robots
    Args:
        N: Number of agents in the simulation
        height, width: The size of the grid to model
    """
    def __init__(self, tiempoMaximo, width, height, num_box):
        """
        Inicializa el modelo. Como argumentos de entrada, requiere:
            - tiempoMaximo: Un tiempo maximo de ejecución
            - width: Un ancho
            - height: Un largo
            - num_box: La cantidad inicial de cajas en el almacen
        """
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
        self.pos_estantes = set()

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
            x = self.random.randrange(self.grid.width)
            y = self.random.randrange(self.grid.height)
            cellmates = self.grid.get_cell_list_contents([(x,y)])
            if (not cellmates):
                estante = Estante(c, self)            
                self.schedule.add(estante)
                self.grid.place_agent(estante, (x, y))
                self.pos_estantes.add((x,y))
                numEstante += 1
                c += 1

        numCajas = 0
        while(numCajas < self.num_box):
            x = self.random.randrange(self.grid.width)
            y = self.random.randrange(self.grid.height)
            cellmates = self.grid.get_cell_list_contents([(x,y)])
            if (not cellmates):
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
        """
        Coteja que no se haya llegado al numero maximo de pasos y 
        que no se hayan recogido todas las cajas
        """
        if self.cont_time == self.max_time:
            self.running = False
        elif self.boxes_recolected == self.num_box:
            self.running = False
        else:
            self.cont_time += 1

    def step(self):
        '''Advance the model by one step.'''
        self.check_steps()
        print(self.pos_estantes)
        self.schedule.step()
