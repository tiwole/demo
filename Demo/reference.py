# -*- coding: utf-8 -*-
"""
Spyder Editor

This is a temporary script file.
"""
import tkinter as tk
from tkinter.messagebox import showerror,showinfo
import ttkbootstrap as ttk
from functools import partial
import mysql.connector as conn
import os
from PIL import Image
from PIL import ImageTk
import random
class tk_capcha(ttk.Frame):
    def __init__(self,i):
        super().__init__(i,width=300, height=300)
        self.grid_propagate(False)
        self.counter=0
        self.labels={}
        self.img={}
        self.rand_list=[[0,0],[150,0],[0,150],[150,150]]
        self.path=self.master.master.path+"\\images\\"
        for j in range(1,5):
            self.labels[str(j)]=ttk.Label(self)
            self.labels[str(j)].bind("<Configure>", partial(self.resize_label_image,k=j))
        self.random_visualize()
    def random_visualize(self):
        if self.counter>=7:
            self.rand_list=[[0,0],[150,0],[0,150],[150,150]]
            self.counter=0
        else:
            random.shuffle(self.rand_list)
            self.counter+=1
        for i in range(1,5):
            self.labels[str(i)].place_forget()
        for i in range(1,5):
            self.labels[str(i)].place(x=self.rand_list[i-1][0],y=self.rand_list[i-1][1])
    def resize_label_image(self,event,k):
        orig_image = Image.open(self.path+".\\"+str(k)+".png")
        orig_w, orig_h = orig_image.size
        scale = min(round(150) / orig_w, round(150) / orig_h)
        new_w = max(1, int(orig_w * scale))
        new_h = max(1, int(orig_h * scale))
        resized_img = orig_image.resize((new_w, new_h), Image.Resampling.LANCZOS)
        self.img[str(k)] = ImageTk.PhotoImage(resized_img) 
        self.labels[str(k)].place_forget()
        self.labels[str(k)].config(image=self.img[str(k)])
        self.labels[str(k)].place(x=self.rand_list[k-1][0],y=self.rand_list[k-1][1])

class admin_frame(ttk.Frame):
    def __init__(self,i):
        super().__init__(i)
        self.users=self.DB_check_many()
        for i in range(10): self.columnconfigure(index=i,weight=1)
        for i in range(11): self.rowconfigure(index=i,weight=1)
        self.rowconfigure(index=12,weight=5)
        self.rowconfigure(index=13,weight=5)
        self.user_add()
        self.user_change()
        self.exitbtn=ttk.Button(self,text="Выйти",command=partial(self.master.change_frame,self,self.master.main_frame))
        self.exitbtn.grid(column=4,row=8,columnspan=1,sticky=tk.NSEW)  
        
    def user_change(self):
        self.lblnewuser_change=ttk.Label(self,text="Изменить пользователя")
        self.lblnewuser_change.grid(column=6,row=0,columnspan=2,sticky=tk.NSEW)
        self.users_var = tk.StringVar(value="") 
        self.passwordvar_change=tk.StringVar(value="")
        self.fiovar_change=tk.StringVar(value="")
        self.roles_var_change = tk.StringVar(value="")
        self.status_var_change = tk.StringVar(value="")
        
        self.users_box = ttk.Combobox(self,textvariable=self.users_var, state="readonly", values=self.users)
        self.users_box.grid(column=6,row=1,columnspan=1,sticky=tk.NSEW)
        self.btnnewuser=ttk.Button(self,text="Изменить пользователя",command=self.user_alter)
        self.btnnewuser.grid(column=6,row=2,columnspan=1,sticky=tk.NSEW)  
        self.btnnewuser=ttk.Button(self,text="Обновить список пользователей",command=self.user_list_configure)
        self.btnnewuser.grid(column=7,row=1,columnspan=1,sticky=tk.NSEW)  
        self.entry_password_change=ttk.Entry(self,textvariable=self.passwordvar_change)
        self.entry_password_change.grid(column=7,row=2,columnspan=1,sticky=tk.NSEW)
        self.entry_fio_change=ttk.Entry(self,textvariable=self.fiovar_change)
        self.entry_fio_change.grid(column=7,row=3,columnspan=1,sticky=tk.NSEW)
        self.roles_box_change = ttk.Combobox(self,textvariable=self.roles_var_change, state="readonly", values=["user","admin"])
        self.roles_box_change.grid(column=7,row=4,columnspan=1,sticky=tk.NSEW)
        self.status_box_change = ttk.Combobox(self,textvariable=self.status_var_change, state="readonly", values=["active","blocked"])
        self.status_box_change.grid(column=7,row=5,columnspan=1,sticky=tk.NSEW)
        self.users_box.bind("<<ComboboxSelected>>", self.users_visualize)
    def user_alter(self):
        try:
            script="UPDATE demo_users SET password = %s, roles=%s, FIO=%s, status = %s  WHERE login=%s"
            values=(str(self.passwordvar_change.get()),
                    str(self.roles_var_change.get()),
                    str(self.fiovar_change.get()),
                    str(self.status_var_change.get()),
                    str(self.users_var.get()))
            cursor = self.master.connection.cursor(script,values)
            cursor.execute(script,values)
            self.master.connection.commit()
            cursor.close()
            showinfo("Успех","Пользователь успешно изменен")
        except:
            showerror("Ошибка","Ошибка! Проверьте данные и попробуйте еще раз")
            self.master.connection_restart()
    def users_visualize(self,event):
        row=self.DB_check_user(str(self.users_var.get()))
        if row!=None:
            self.passwordvar_change.set(row[2])
            self.fiovar_change.set(row[4])
            self.roles_var_change.set(row[3])
            self.status_var_change.set(row[5])
    def user_list_configure(self):
        self.users=self.DB_check_many()
        self.users_box.configure(values=self.users)
    def DB_check_user(self,value):
        script="""Select * FROM demo_users WHERE login=%s;"""
        try:
            cursor = self.master.connection.cursor()
            cursor.execute(script,(value,))
            row=cursor.fetchone()
            cursor.fetchall()
            cursor.close()    
            return row
        except:
            showerror("Ошибка","Возникла ошибка с связью с БД! Пожалуйста,\nпопробуйте еще раз или перезапустите приложение.")
            self.master.connection_restart()
    def DB_check_many(self):
        script="""Select login FROM demo_users;"""
        try:
            cursor = self.master.connection.cursor()
            cursor.execute(script)
            rows=cursor.fetchall()
            cursor.close()    
            return rows
        except:
            showerror("Ошибка","Возникла ошибка с связью с БД! Пожалуйста,\nпопробуйте еще раз или перезапустите приложение.")
            self.master.connection_restart()
    def user_add(self):
        self.loginvar=tk.StringVar(value="")
        self.passwordvar=tk.StringVar(value="")
        self.fiovar=tk.StringVar(value="")
        self.lblnewuser=ttk.Label(self,text="Создать нового пользователя")
        self.lblnewuser.grid(column=1,row=1,columnspan=1,sticky=tk.NSEW)
        self.loginlbl=ttk.Label(self,text="Логин:")
        self.loginlbl.grid(column=1,row=2,columnspan=1,sticky=tk.NSEW)
        self.entry_login=ttk.Entry(self,textvariable=self.loginvar)
        self.entry_login.grid(column=1,row=3,columnspan=1,sticky=tk.NSEW)
        self.passwordlbl=ttk.Label(self,text="Пароль:")
        self.passwordlbl.grid(column=1,row=4,columnspan=1,sticky=tk.NSEW)
        self.entry_password=ttk.Entry(self,textvariable=self.passwordvar)
        self.entry_password.grid(column=1,row=5,columnspan=1,sticky=tk.NSEW)
        self.fiolbl=ttk.Label(self,text="ФИО:")
        self.fiolbl.grid(column=1,row=6,columnspan=1,sticky=tk.NSEW)
        self.entry_fio=ttk.Entry(self,textvariable=self.fiovar)
        self.entry_fio.grid(column=1,row=7,columnspan=1,sticky=tk.NSEW)
        self.roles_var = tk.StringVar(value="user") 
        self.roles_box = ttk.Combobox(self,textvariable=self.roles_var, state="readonly", values=["user","admin"])
        self.roles_box.grid(column=1,row=8,columnspan=1,sticky=tk.NSEW)
        self.status_var = tk.StringVar(value="active")
        self.status_box = ttk.Combobox(self,textvariable=self.status_var, state="readonly", values=["active","blocked"])
        self.status_box.grid(column=1,row=9,columnspan=1,sticky=tk.NSEW)
        self.btnnewuser=ttk.Button(self,text="Добавить пользователя",command=self.register_user)
        self.btnnewuser.grid(column=1,row=10,columnspan=1,sticky=tk.NSEW)
    def register_user(self):
        try:
            if (len(self.loginvar.get())==0 or len(self.passwordvar.get())==0 or 
            len(self.roles_var.get())==0 or len(self.fiovar.get())==0 or 
            len(self.status_var.get())==0):
                showerror("Ошибка","Поля не могут быть пустыми")
            else:
                row=self.master.main_frame.DB_check("""Select * FROM demo_users WHERE login=%s;""",(str(self.loginvar.get()),))
                if row!=None:
                    showerror("Ошибка","Логин занят! Попробуйте другой логин")
                else:
                    try:
                        script="INSERT INTO demo_users VALUES (Null, %s, %s, %s, %s, %s);"
                        values=(str(self.loginvar.get()), str(self.passwordvar.get()),
                                str(self.roles_var.get()),
                                str(self.fiovar.get()), str(self.status_var.get()))
                        cursor = self.master.connection.cursor(script,values)
                        cursor.execute(script,values)
                        self.master.connection.commit()
                        cursor.close()
                        showinfo("Успех","Пользователь успешно зарегистрирован")
                    except:
                        showerror("Ошибка","Ошибка! Проверьте данные и попробуйте еще раз")
                        self.master.connection_restart()
        except:
            showerror("Ошибка","Ошибка! Проверьте данные и попробуйте еще раз")
class user_frame(ttk.Frame):
    def __init__(self,i):
        super().__init__(i)
        for i in range(10): self.columnconfigure(index=i,weight=1)
        for i in range(11): self.rowconfigure(index=i,weight=1)
        ttk.Label(self,text="Вы успешно вошли как пользователь. Это окно-заглушка, для для дальнейшей реализации функций пользователя").grid(column=0,row=0,columnspan=10,sticky=tk.NSEW)
        for i in range(1,10):
            for k in range(10):
                ttk.Button(self,text=str(k)+"-"+str(i)).grid(column=k,row=i,columnspan=1,sticky=tk.NSEW)
        self.exitbtn=ttk.Button(self,text="Выйти",command=partial(self.master.change_frame,self,self.master.main_frame))
        self.exitbtn.grid(column=5,row=10,columnspan=1,sticky=tk.NSEW) 
class tk_main_frame(ttk.Frame):
    def __init__(self,i):
        super().__init__(i)
        for i in range(4): self.columnconfigure(index=i,weight=1)
        for i in range(10): self.rowconfigure(index=i,weight=1)
        self.capchaerror=0
        self.loginvar=tk.StringVar(value="")
        self.passwordvar=tk.StringVar(value="")
        self.loginlbl=ttk.Label(self,text="Логин:")
        self.loginlbl.grid(column=1,row=1,columnspan=2,sticky=tk.NSEW)
        self.entry_login=ttk.Entry(self,textvariable=self.loginvar)
        self.entry_login.grid(column=1,row=2,columnspan=2,sticky=tk.NSEW)
        self.passwordlbl=ttk.Label(self,text="Пароль:")
        self.passwordlbl.grid(column=1,row=3,columnspan=2,sticky=tk.NSEW)
        self.entry_password=ttk.Entry(self,textvariable=self.passwordvar)
        self.entry_password.grid(column=1,row=4,columnspan=2,sticky=tk.NSEW)
        self.button_continue=ttk.Button(self,text="Войти",command=self.login_check)
        self.button_continue.grid(column=1,row=8,columnspan=2,sticky=tk.NSEW)
        self.capcha_im=tk_capcha(self)
        self.capcha_im.grid(column=1,row=5,columnspan=2,rowspan=2,sticky=tk.NSEW,padx=5,pady=5)
        self.capchastart=ttk.Button(self,text="Решить капчу",command=self.capcha_im.random_visualize)
        self.capchastart.grid(column=1,row=7,columnspan=2,sticky=tk.NSEW)
    def DB_check(self,script,values):
        try:
            cursor = self.master.connection.cursor()
            cursor.execute(script,values)
            row=cursor.fetchone()
            cursor.fetchall()
            cursor.close()    
            return row
        except:
            showerror("Ошибка","Возникла ошибка с связью с БД! Пожалуйста,\nпопробуйте еще раз или перезапустите приложение.")
            self.master.connection_restart()
    def DB_alter(self,script,values):
        try:
            cursor = self.master.connection.cursor(script,values)
            cursor.execute(script,values)
            self.master.connection.commit()
            cursor.close()
        except:
            showerror("Ошибка","Возникла ошибка с связью с БД! Пожалуйста,\nпопробуйте еще раз или перезапустите приложение.")
            self.master.connection_restart()
    def login_check(self):
        if len(self.loginvar.get())==0 or len(self.passwordvar.get())==0:
            showerror("Ошибка","Логин и пароль не могут быть пустыми")
        else:
            row=self.DB_check("""Select * FROM demo_users WHERE login=%s;""",(str(self.loginvar.get()),))
            if row==None:
                showerror("Ошибка","Вы ввели неверный логин.\nПожалуйста проверьте еще раз введенные данные")
            else:
                row=self.DB_check("""Select * FROM demo_users WHERE login=%s and password=%s;""",(str(self.loginvar.get()),str(self.passwordvar.get())))
                if row==None or self.capcha_im.rand_list!=[[0,0],[150,0],[0,150],[150,150]]:
                    showerror("Ошибка","Вы ввели неверный пароль или не решили капчу.\nПожалуйста проверьте еще раз введенные данные")
                    self.capchaerror+=1
                    if self.capchaerror>=3:
                        self.DB_alter("UPDATE demo_users SET status = 'blocked' WHERE login=%s",(self.loginvar.get()));
                else:   
                    if row[5]=="blocked":
                        showerror("Ошибка","Вы заблокированы. Обратитесь к администратору")
                    else:
                        self.login_into(row[2])
                
    def login_into(self,role):
        if role=="user":
            self.master.change_frame(self.master.main_frame,self.master.user_frame)
        elif role=="admin":
            self.master.change_frame(self.master.main_frame,self.master.admin_frame)
        else:
            showerror("Ошибка","Возникла ошибка с ролью. Пожалуйста, обратитесь к администратору")
class tk_mainclass(ttk.Window):
    def __init__(self,path):
        super().__init__()
        self.path=path
        self.title("Aytorize_System")
        self.geometry("1200x720")
        self.resizable(True,True)
        self.protocol("WM_DELETE_WINDOW",self.on_closing)
        self.update_idletasks()
        self.connection_create()
        self.mainframe()
        self.otherframes()
        self.main_menu_add()
    def change_frame(self,old_frame,new_frame):
        old_frame.pack_forget()
        new_frame.pack(fill='both', expand=True)
    def mainframe(self):
        self.main_frame=tk_main_frame(self)
        self.main_frame.pack(fill='both', expand=True)
    def otherframes(self):
        self.user_frame=user_frame(self)
        self.admin_frame=admin_frame(self)
    def connection_restart(self):
        self.connection.close()
        self.connection_create()
    def connection_create(self):
        try:
            self.connection=conn.connect(
                host="quaponumeno.beget.app",
                user="default-db",
                password="14112000Mavrin@",
                database="default-db",
                port="3306")
        except:
            showerror("Ошибка","Возникла ошибка с связью с БД! Пожалуйста,\nпопробуйте проверить сеть и перезапустить приложение.")
    def on_closing(self):
        self.connection.close()
        self.destroy()
    def theme_use(self,i):
        self.style.theme_use(i)
    def main_menu_add(self):
        self.theme_menu=tk.Menu()
        for i in self.style.theme_names():
            self.theme_menu.add_command(label=i,command=partial(self.theme_use,i))
        self.main_menu=tk.Menu()
        self.main_menu.add_cascade(label="Theme", menu=self.theme_menu)
        self.config(menu=self.main_menu)
def main():
    path=os.path.dirname(os.path.abspath(__file__))
    root=tk_mainclass(path)
    root.mainloop()
    del root

if __name__== "__main__":
    main()
