import { DataTypes, Model, Sequelize } from "@sequelize/core";
export const db = new Sequelize('sqlite://database.sqlite', {logging: false});

export class User extends Model {
    declare username: string
    declare password: string
    declare up_time: string
    declare ip: string
}

User.init({
    username: {
        type: DataTypes.STRING,
        allowNull: false,
        unique: true,
        primaryKey: true
    },
    password: {
        type: DataTypes.STRING
    },
    up_time: DataTypes.INTEGER,
    ip: DataTypes.STRING
}, {sequelize: db, modelName: 'User'});
