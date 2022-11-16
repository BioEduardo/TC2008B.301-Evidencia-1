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
        self.has_box = False
 
    # Define el movimiento aleatorio de los agentes
    def move(self):
        possible_steps = self.model.grid.get_neighborhood(
            self.pos,
            moore=True,
            include_center=False)
        new_position = self.random.choice(possible_steps)
        cellmates = self.model.grid.get_cell_list_contents([new_position])
        for element in cellmates:
            if isinstance(element, Robot) or isinstance(element, Pared) or isinstance(element, Puerta):
                new_position = self.pos
        self.model.grid.move_agent(self, new_position)

    def moveToEstante(self):
        posX = self.pos[0]
        posY = self.pos[1]
        if posX != 1 and posY != 1:
            new_position = (posX-1,posY)
        elif posX == 1 and posY != 1:
            new_position = (posX,posY-1)
        elif posX == 1 and posY == 1:
            new_position = (posX+1,posY)
        elif posY == 1:
            new_position = (posX+1,posY)
        else:
            new_position = (posX+1,posY)

        cellmates = self.model.grid.get_cell_list_contents([new_position])
        for element in cellmates:
            if isinstance(element, Robot) or isinstance(element, Pared) or isinstance(element, Puerta):
                new_position = self.pos

        self.model.grid.move_agent(self, new_position)
    
    def colocarCajaEstante(self, estante):
        self.model.boxes_recolected += 1
        estante.current_boxes += 1
        self.color = "green"        
        self.has_box = False

    def recogerCaja(self, caja):
        caja.color = "white"
        self.color = "orange"
        self.has_box = True

    # Representa un paso, donde se coteja si la celda esta limpia
    def step(self):
        if self.has_box:
            self.moveToEstante()
        else:
            self.move()
        self.movements += 1
        cellmates = self.model.grid.get_cell_list_contents([self.pos])
        for agent in cellmates:
            if type(agent) is Estante:
                if agent.current_boxes < 5 and self.has_box:
                    self.colocarCajaEstante(agent)
            elif type(agent) is Caja and not self.has_box and agent.color == "red":
                self.recogerCaja(agent)
                

# Representa al agente caja
class Caja(Agent):

    RECOGIDO = 1
    NO_RECOGIDO = 0

    # Inicializa sus valores de instacia
    def __init__(self, unique_id, model, init_status=NO_RECOGIDO):
        super().__init__(unique_id, model)
        self.color = "red"
        self.status = init_status

    # Representa un paso
    def step(self):
        pass


class Estante(Agent):

    # Inicializa sus valores de instacia
    def __init__(self, unique_id, model):
        super().__init__(unique_id, model)
        self.current_boxes = 0
        self.max_boxes = 5
    
    # Representa un paso
    def step(self):
        pass


class Pared(Agent):
    """
    Pared agent. Just to add Pared to the grid.
    """
    def __init__(self, unique_id, model):
        super().__init__(unique_id, model)

    def step(self):
        pass


class Puerta(Agent):
    """
    Puerta agent. Just to add Puerta to the grid.
    """
    def __init__(self, unique_id, model):
        super().__init__(unique_id, model)

    def step(self):
        pass
