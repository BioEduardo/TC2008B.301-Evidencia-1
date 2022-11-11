#----------------------------------------------------------
# M1. Actividad
# Este programa representa a los agentes Robot y Basura
# 
# Date: 11-Nov-2022
# Authors:
#           Eduardo Joel Cortez Valente A01746664
#           Paulo Ogando Gulias A01751587
#----------------------------------------------------------

from mesa import Agent

# Representacion de un robot agente
class Robot(Agent):

    # Inicializa los valores
    def __init__(self, unique_id, model):
        super().__init__(unique_id, model)
        self.movements = 0
        self.color = "green"
        self.state = False
 
    # Define el movimiento aleatorio de los agentes
    def move(self):
        possible_steps = self.model.grid.get_neighborhood(
            self.pos,
            moore=True,
            include_center=False)
        new_position = self.random.choice(possible_steps)
        self.model.grid.move_agent(self, new_position)
    
    def colocarCajaEstante(self, estante):
        estante.num_boxes += 1
        self.color = "green"        
        self.state = False

    def recogerCaja(self, caja):
        caja.color = "white"
        self.color = "orange"
        self.state = True

    # Representa un paso, donde se coteja si la celda esta limpia
    def step(self):
        self.move()
        self.movements += 1
        cellmates = self.model.grid.get_cell_list_contents([self.pos])
        for agent in cellmates:
            if type(agent) is Estante:
                if agent.num_boxes < 5 and self.state:
                    self.colocarCajaEstante(agent)
            elif type(agent) is Caja and not self.state and agent.color == "red":
                self.recogerCaja(agent)
   


# Representa al agente caja
class Caja(Agent):

    # Inicializa sus valores de instacia
    def __init__(self, unique_id, model):
        super().__init__(unique_id, model)
        self.state = 0
        self.color = "red"

    # Representa un paso
    def step(self):
        pass

class Estante(Agent):
    
        # Inicializa sus valores de instacia
        def __init__(self, unique_id, model):
            super().__init__(unique_id, model)
            self.num_boxes = 0
    
        # Representa un paso
        def step(self):
            pass